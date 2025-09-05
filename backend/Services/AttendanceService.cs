using Microsoft.EntityFrameworkCore;
using AutoMapper;
using backend.Data;
using backend.Models;
using backend.DTOs;

namespace backend.Services
{
    public interface IAttendanceService
    {
        // Check-in/Check-out
        Task<AttendanceDto> CheckInAsync(int employeeId, CheckInDto checkInDto);
        Task<AttendanceDto> CheckOutAsync(int employeeId, CheckOutDto checkOutDto);
        Task<AttendanceDto> RecordBreakTimeAsync(int employeeId, BreakTimeDto breakTimeDto);

        // Attendance Management
        Task<IEnumerable<AttendanceDto>> GetAttendancesAsync(AttendanceReportFilterDto filter);
        Task<AttendanceDto?> GetAttendanceByIdAsync(int id);
        Task<AttendanceDto?> GetTodayAttendanceAsync(int employeeId);
        Task<AttendanceDto?> UpdateAttendanceAsync(int id, AttendanceApprovalDto approvalDto);
        Task<bool> DeleteAttendanceAsync(int id);

        // Attendance Details
        Task<IEnumerable<AttendanceDetailDto>> GetAttendanceDetailsAsync(int attendanceId);

        // Reports and Analytics
        Task<AttendanceSummaryDto?> GetMonthlySummaryAsync(int employeeId, int year, int month);
        Task<IEnumerable<AttendanceSummaryDto>> GetDepartmentSummaryAsync(int departmentId, int year, int month);
        Task<DailyAttendanceReportDto> GetDailyReportAsync(DateTime date, int? departmentId = null);
        Task<EmployeeAttendanceHistoryDto> GetEmployeeAttendanceHistoryAsync(int employeeId, DateTime startDate, DateTime endDate);

        // Bulk Operations
        Task<bool> GenerateMonthlyAttendanceSummaryAsync(int year, int month);
        Task<bool> ApproveMultipleAttendancesAsync(List<int> attendanceIds, int approverId, string? notes = null);

        // Helper Methods
        Task<bool> HasCheckedInTodayAsync(int employeeId);
        Task<bool> HasCheckedOutTodayAsync(int employeeId);
        Task<AttendanceStatus> CalculateAttendanceStatusAsync(int attendanceId);
        Task<int> CalculateWorkingMinutesAsync(int attendanceId);
    }

    public class AttendanceService : IAttendanceService
    {
        private readonly HrmDbContext _context;
        private readonly IMapper _mapper;

        public AttendanceService(HrmDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AttendanceDto> CheckInAsync(int employeeId, CheckInDto checkInDto)
        {
            var today = checkInDto.CheckInTime.Date;

            // Check if already checked in today
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceDate.Date == today);

            if (existingAttendance?.CheckInTime != null)
            {
                throw new InvalidOperationException("Employee has already checked in today");
            }

            // Get employee's schedule for today
            var schedule = await _context.WorkSchedules
                .Include(ws => ws.WorkShift)
                .FirstOrDefaultAsync(ws => ws.EmployeeId == employeeId && ws.WorkDate.Date == today);

            if (schedule == null)
            {
                // Try to get default shift assignment
                var shiftAssignment = await _context.EmployeeShiftAssignments
                    .Include(esa => esa.WorkShift)
                    .Where(esa => esa.EmployeeId == employeeId)
                    .Where(esa => esa.EffectiveFrom <= today && (esa.EffectiveTo == null || esa.EffectiveTo >= today))
                    .Where(esa => esa.IsDefaultShift)
                    .FirstOrDefaultAsync();

                if (shiftAssignment == null)
                {
                    throw new InvalidOperationException("No work schedule or shift assignment found for today");
                }

                schedule = new WorkSchedule
                {
                    EmployeeId = employeeId,
                    WorkShiftId = shiftAssignment.WorkShiftId,
                    WorkDate = today,
                    WorkShift = shiftAssignment.WorkShift
                };
            }

            var attendance = existingAttendance ?? new Attendance
            {
                EmployeeId = employeeId,
                AttendanceDate = today,
                WorkShiftId = schedule.WorkShiftId,
                CreatedAt = DateTime.UtcNow
            };

            // Set check-in details
            attendance.CheckInTime = checkInDto.CheckInTime;
            attendance.CheckInLatitude = checkInDto.Latitude;
            attendance.CheckInLongitude = checkInDto.Longitude;
            attendance.CheckInLocation = checkInDto.Location;
            attendance.CheckInPhotoUrl = checkInDto.PhotoUrl;
            attendance.Notes = checkInDto.Notes;

            // Calculate late minutes
            var expectedStartTime = DateTime.Today.Add(schedule.WorkShift.StartTime);
            var flexibleMinutes = schedule.WorkShift.FlexibleMinutes ?? 0;
            var allowedStartTime = expectedStartTime.AddMinutes(flexibleMinutes);

            if (checkInDto.CheckInTime > allowedStartTime)
            {
                attendance.LateMinutes = (int)(checkInDto.CheckInTime - allowedStartTime).TotalMinutes;
                attendance.Status = AttendanceStatus.Late;
            }
            else
            {
                attendance.LateMinutes = 0;
                attendance.Status = AttendanceStatus.OnTime;
            }

            if (existingAttendance == null)
            {
                _context.Attendances.Add(attendance);
            }
            else
            {
                attendance.UpdatedAt = DateTime.UtcNow;
            }

            // Add attendance detail
            var attendanceDetail = new AttendanceDetail
            {
                Attendance = attendance,
                Type = AttendanceType.CheckIn,
                Timestamp = checkInDto.CheckInTime,
                Latitude = checkInDto.Latitude,
                Longitude = checkInDto.Longitude,
                Location = checkInDto.Location,
                DeviceId = checkInDto.DeviceId,
                DeviceType = checkInDto.DeviceType,
                PhotoUrl = checkInDto.PhotoUrl,
                Notes = checkInDto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.AttendanceDetails.Add(attendanceDetail);
            await _context.SaveChangesAsync();

            return await GetAttendanceByIdAsync(attendance.Id) ?? 
                   throw new InvalidOperationException("Failed to retrieve attendance record");
        }

        public async Task<AttendanceDto> CheckOutAsync(int employeeId, CheckOutDto checkOutDto)
        {
            var today = checkOutDto.CheckOutTime.Date;

            var attendance = await _context.Attendances
                .Include(a => a.WorkShift)
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceDate.Date == today);

            if (attendance == null || attendance.CheckInTime == null)
            {
                throw new InvalidOperationException("Employee must check in before checking out");
            }

            if (attendance.CheckOutTime != null)
            {
                throw new InvalidOperationException("Employee has already checked out today");
            }

            // Set check-out details
            attendance.CheckOutTime = checkOutDto.CheckOutTime;
            attendance.CheckOutLatitude = checkOutDto.Latitude;
            attendance.CheckOutLongitude = checkOutDto.Longitude;
            attendance.CheckOutLocation = checkOutDto.Location;
            attendance.CheckOutPhotoUrl = checkOutDto.PhotoUrl;
            attendance.UpdatedAt = DateTime.UtcNow;

            // Calculate working time and status
            var workShift = attendance.WorkShift;
            var expectedEndTime = DateTime.Today.Add(workShift.EndTime);
            var flexibleMinutes = workShift.FlexibleMinutes ?? 0;
            var allowedEndTime = expectedEndTime.AddMinutes(-flexibleMinutes);

            // Calculate total working minutes
            var totalMinutes = (int)(checkOutDto.CheckOutTime - attendance.CheckInTime.Value).TotalMinutes;
            
            // Subtract break time if recorded
            if (attendance.BreakStartTime.HasValue && attendance.BreakEndTime.HasValue)
            {
                var breakMinutes = (int)(attendance.BreakEndTime.Value - attendance.BreakStartTime.Value).TotalMinutes;
                attendance.BreakMinutes = breakMinutes;
                totalMinutes -= breakMinutes;
            }

            attendance.TotalWorkingMinutes = totalMinutes;

            // Calculate early leave
            if (checkOutDto.CheckOutTime < allowedEndTime)
            {
                attendance.EarlyLeaveMinutes = (int)(allowedEndTime - checkOutDto.CheckOutTime).TotalMinutes;
                if (attendance.Status != AttendanceStatus.Late)
                {
                    attendance.Status = AttendanceStatus.Early;
                }
            }

            // Calculate overtime
            if (checkOutDto.CheckOutTime > expectedEndTime && workShift.AllowOvertime)
            {
                attendance.OvertimeMinutes = (int)(checkOutDto.CheckOutTime - expectedEndTime).TotalMinutes;
                if (attendance.Status == AttendanceStatus.OnTime)
                {
                    attendance.Status = AttendanceStatus.Overtime;
                }
            }

            // Add attendance detail
            var attendanceDetail = new AttendanceDetail
            {
                AttendanceId = attendance.Id,
                Type = AttendanceType.CheckOut,
                Timestamp = checkOutDto.CheckOutTime,
                Latitude = checkOutDto.Latitude,
                Longitude = checkOutDto.Longitude,
                Location = checkOutDto.Location,
                DeviceId = checkOutDto.DeviceId,
                DeviceType = checkOutDto.DeviceType,
                PhotoUrl = checkOutDto.PhotoUrl,
                Notes = checkOutDto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.AttendanceDetails.Add(attendanceDetail);
            await _context.SaveChangesAsync();

            return await GetAttendanceByIdAsync(attendance.Id) ?? 
                   throw new InvalidOperationException("Failed to retrieve attendance record");
        }

        public async Task<AttendanceDto> RecordBreakTimeAsync(int employeeId, BreakTimeDto breakTimeDto)
        {
            var today = breakTimeDto.Timestamp.Date;

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceDate.Date == today);

            if (attendance == null || attendance.CheckInTime == null)
            {
                throw new InvalidOperationException("Employee must check in before recording break time");
            }

            if (breakTimeDto.Type == AttendanceType.BreakStart)
            {
                if (attendance.BreakStartTime != null)
                {
                    throw new InvalidOperationException("Break start time already recorded");
                }
                attendance.BreakStartTime = breakTimeDto.Timestamp;
            }
            else if (breakTimeDto.Type == AttendanceType.BreakEnd)
            {
                if (attendance.BreakStartTime == null)
                {
                    throw new InvalidOperationException("Must record break start time first");
                }
                if (attendance.BreakEndTime != null)
                {
                    throw new InvalidOperationException("Break end time already recorded");
                }
                attendance.BreakEndTime = breakTimeDto.Timestamp;
            }

            attendance.UpdatedAt = DateTime.UtcNow;

            // Add attendance detail
            var attendanceDetail = new AttendanceDetail
            {
                AttendanceId = attendance.Id,
                Type = breakTimeDto.Type,
                Timestamp = breakTimeDto.Timestamp,
                Latitude = breakTimeDto.Latitude,
                Longitude = breakTimeDto.Longitude,
                Location = breakTimeDto.Location,
                Notes = breakTimeDto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.AttendanceDetails.Add(attendanceDetail);
            await _context.SaveChangesAsync();

            return await GetAttendanceByIdAsync(attendance.Id) ?? 
                   throw new InvalidOperationException("Failed to retrieve attendance record");
        }

        public async Task<IEnumerable<AttendanceDto>> GetAttendancesAsync(AttendanceReportFilterDto filter)
        {
            var query = _context.Attendances
                .Include(a => a.Employee).ThenInclude(e => e.User)
                .Include(a => a.WorkShift)
                .Include(a => a.ApprovedBy).ThenInclude(ab => ab!.User)
                .AsQueryable();

            if (filter.EmployeeId.HasValue)
                query = query.Where(a => a.EmployeeId == filter.EmployeeId.Value);

            if (filter.DepartmentId.HasValue)
                query = query.Where(a => a.Employee.DepartmentId == filter.DepartmentId.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(a => a.AttendanceDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(a => a.AttendanceDate <= filter.EndDate.Value);

            if (filter.Status.HasValue)
                query = query.Where(a => a.Status == filter.Status.Value);

            var attendances = await query
                .OrderByDescending(a => a.AttendanceDate)
                .ThenBy(a => a.Employee.User.FullName)
                .ToListAsync();

            return attendances.Select(a => new AttendanceDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.User.FullName,
                AttendanceDate = a.AttendanceDate,
                WorkShiftId = a.WorkShiftId,
                WorkShiftName = a.WorkShift.Name,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                BreakStartTime = a.BreakStartTime,
                BreakEndTime = a.BreakEndTime,
                CheckInLocation = a.CheckInLocation,
                CheckOutLocation = a.CheckOutLocation,
                TotalWorkingMinutes = a.TotalWorkingMinutes,
                BreakMinutes = a.BreakMinutes,
                LateMinutes = a.LateMinutes,
                EarlyLeaveMinutes = a.EarlyLeaveMinutes,
                OvertimeMinutes = a.OvertimeMinutes,
                Status = a.Status,
                Notes = a.Notes,
                ManagerNotes = a.ManagerNotes,
                ApprovedByName = a.ApprovedBy?.User?.FullName,
                ApprovedAt = a.ApprovedAt
            });
        }

        public async Task<AttendanceDto?> GetAttendanceByIdAsync(int id)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Employee).ThenInclude(e => e.User)
                .Include(a => a.WorkShift)
                .Include(a => a.ApprovedBy).ThenInclude(ab => ab!.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null) return null;

            return new AttendanceDto
            {
                Id = attendance.Id,
                EmployeeId = attendance.EmployeeId,
                EmployeeName = attendance.Employee.User.FullName,
                AttendanceDate = attendance.AttendanceDate,
                WorkShiftId = attendance.WorkShiftId,
                WorkShiftName = attendance.WorkShift.Name,
                CheckInTime = attendance.CheckInTime,
                CheckOutTime = attendance.CheckOutTime,
                BreakStartTime = attendance.BreakStartTime,
                BreakEndTime = attendance.BreakEndTime,
                CheckInLocation = attendance.CheckInLocation,
                CheckOutLocation = attendance.CheckOutLocation,
                TotalWorkingMinutes = attendance.TotalWorkingMinutes,
                BreakMinutes = attendance.BreakMinutes,
                LateMinutes = attendance.LateMinutes,
                EarlyLeaveMinutes = attendance.EarlyLeaveMinutes,
                OvertimeMinutes = attendance.OvertimeMinutes,
                Status = attendance.Status,
                Notes = attendance.Notes,
                ManagerNotes = attendance.ManagerNotes,
                ApprovedByName = attendance.ApprovedBy?.User?.FullName,
                ApprovedAt = attendance.ApprovedAt
            };
        }

        public async Task<AttendanceDto?> GetTodayAttendanceAsync(int employeeId)
        {
            var today = DateTime.Today;
            var attendance = await _context.Attendances
                .Include(a => a.Employee).ThenInclude(e => e.User)
                .Include(a => a.WorkShift)
                .Include(a => a.ApprovedBy).ThenInclude(ab => ab!.User)
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceDate.Date == today);

            if (attendance == null) return null;

            return new AttendanceDto
            {
                Id = attendance.Id,
                EmployeeId = attendance.EmployeeId,
                EmployeeName = attendance.Employee.User.FullName,
                AttendanceDate = attendance.AttendanceDate,
                WorkShiftId = attendance.WorkShiftId,
                WorkShiftName = attendance.WorkShift.Name,
                CheckInTime = attendance.CheckInTime,
                CheckOutTime = attendance.CheckOutTime,
                BreakStartTime = attendance.BreakStartTime,
                BreakEndTime = attendance.BreakEndTime,
                CheckInLocation = attendance.CheckInLocation,
                CheckOutLocation = attendance.CheckOutLocation,
                TotalWorkingMinutes = attendance.TotalWorkingMinutes,
                BreakMinutes = attendance.BreakMinutes,
                LateMinutes = attendance.LateMinutes,
                EarlyLeaveMinutes = attendance.EarlyLeaveMinutes,
                OvertimeMinutes = attendance.OvertimeMinutes,
                Status = attendance.Status,
                Notes = attendance.Notes,
                ManagerNotes = attendance.ManagerNotes,
                ApprovedByName = attendance.ApprovedBy?.User?.FullName,
                ApprovedAt = attendance.ApprovedAt
            };
        }

        public async Task<AttendanceDto?> UpdateAttendanceAsync(int id, AttendanceApprovalDto approvalDto)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null) return null;

            attendance.Status = approvalDto.Status;
            attendance.ManagerNotes = approvalDto.ManagerNotes;
            attendance.UpdatedAt = DateTime.UtcNow;

            if (approvalDto.Status == AttendanceStatus.Approved)
            {
                attendance.ApprovedAt = DateTime.UtcNow;
                // ApprovedById should be set from the current user context
            }

            await _context.SaveChangesAsync();
            return await GetAttendanceByIdAsync(id);
        }

        public async Task<bool> DeleteAttendanceAsync(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null) return false;

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AttendanceDetailDto>> GetAttendanceDetailsAsync(int attendanceId)
        {
            var details = await _context.AttendanceDetails
                .Where(ad => ad.AttendanceId == attendanceId)
                .OrderBy(ad => ad.Timestamp)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AttendanceDetailDto>>(details);
        }

        public async Task<AttendanceSummaryDto?> GetMonthlySummaryAsync(int employeeId, int year, int month)
        {
            var summary = await _context.AttendanceSummaries
                .Include(s => s.Employee).ThenInclude(e => e.User)
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.Year == year && s.Month == month);

            if (summary == null) return null;

            return new AttendanceSummaryDto
            {
                Id = summary.Id,
                EmployeeId = summary.EmployeeId,
                EmployeeName = summary.Employee.User.FullName,
                Year = summary.Year,
                Month = summary.Month,
                TotalWorkingDays = summary.TotalWorkingDays,
                ActualWorkingDays = summary.ActualWorkingDays,
                AbsentDays = summary.AbsentDays,
                LateDays = summary.LateDays,
                EarlyLeaveDays = summary.EarlyLeaveDays,
                TotalWorkingMinutes = summary.TotalWorkingMinutes,
                StandardWorkingMinutes = summary.StandardWorkingMinutes,
                OvertimeMinutes = summary.OvertimeMinutes,
                LateMinutes = summary.LateMinutes,
                EarlyLeaveMinutes = summary.EarlyLeaveMinutes,
                VacationDays = summary.VacationDays,
                SickLeaveDays = summary.SickLeaveDays,
                PersonalLeaveDays = summary.PersonalLeaveDays
            };
        }

        public async Task<IEnumerable<AttendanceSummaryDto>> GetDepartmentSummaryAsync(int departmentId, int year, int month)
        {
            var summaries = await _context.AttendanceSummaries
                .Include(s => s.Employee).ThenInclude(e => e.User)
                .Where(s => s.Employee.DepartmentId == departmentId && s.Year == year && s.Month == month)
                .ToListAsync();

            return summaries.Select(s => new AttendanceSummaryDto
            {
                Id = s.Id,
                EmployeeId = s.EmployeeId,
                EmployeeName = s.Employee.User.FullName,
                Year = s.Year,
                Month = s.Month,
                TotalWorkingDays = s.TotalWorkingDays,
                ActualWorkingDays = s.ActualWorkingDays,
                AbsentDays = s.AbsentDays,
                LateDays = s.LateDays,
                EarlyLeaveDays = s.EarlyLeaveDays,
                TotalWorkingMinutes = s.TotalWorkingMinutes,
                StandardWorkingMinutes = s.StandardWorkingMinutes,
                OvertimeMinutes = s.OvertimeMinutes,
                LateMinutes = s.LateMinutes,
                EarlyLeaveMinutes = s.EarlyLeaveMinutes,
                VacationDays = s.VacationDays,
                SickLeaveDays = s.SickLeaveDays,
                PersonalLeaveDays = s.PersonalLeaveDays
            });
        }

        public async Task<DailyAttendanceReportDto> GetDailyReportAsync(DateTime date, int? departmentId = null)
        {
            var query = _context.Attendances
                .Include(a => a.Employee).ThenInclude(e => e.User)
                .Include(a => a.WorkShift)
                .Where(a => a.AttendanceDate.Date == date.Date);

            if (departmentId.HasValue)
            {
                query = query.Where(a => a.Employee.DepartmentId == departmentId.Value);
            }

            var attendances = await query.ToListAsync();
            
            var totalEmployeesQuery = _context.Employees.AsQueryable();
            if (departmentId.HasValue)
            {
                totalEmployeesQuery = totalEmployeesQuery.Where(e => e.DepartmentId == departmentId.Value);
            }
            
            var totalEmployees = await totalEmployeesQuery.CountAsync();

            return new DailyAttendanceReportDto
            {
                Date = date,
                TotalEmployees = totalEmployees,
                PresentEmployees = attendances.Count(a => a.CheckInTime != null),
                AbsentEmployees = totalEmployees - attendances.Count(a => a.CheckInTime != null),
                LateEmployees = attendances.Count(a => a.LateMinutes > 0),
                EarlyLeaveEmployees = attendances.Count(a => a.EarlyLeaveMinutes > 0),
                AttendanceRecords = attendances.Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.Employee.User.FullName,
                    AttendanceDate = a.AttendanceDate,
                    WorkShiftId = a.WorkShiftId,
                    WorkShiftName = a.WorkShift.Name,
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    Status = a.Status,
                    LateMinutes = a.LateMinutes,
                    EarlyLeaveMinutes = a.EarlyLeaveMinutes,
                    OvertimeMinutes = a.OvertimeMinutes
                }).ToList()
            };
        }

        public async Task<EmployeeAttendanceHistoryDto> GetEmployeeAttendanceHistoryAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
            {
                throw new ArgumentException("Employee not found", nameof(employeeId));
            }

            var attendances = await GetAttendancesAsync(new AttendanceReportFilterDto
            {
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate
            });

            // Calculate summary
            var attendanceList = attendances.ToList();
            var summary = new AttendanceSummaryDto
            {
                EmployeeId = employeeId,
                EmployeeName = employee.User.FullName,
                TotalWorkingDays = (endDate - startDate).Days + 1,
                ActualWorkingDays = attendanceList.Count(a => a.CheckInTime != null),
                LateDays = attendanceList.Count(a => a.LateMinutes > 0),
                EarlyLeaveDays = attendanceList.Count(a => a.EarlyLeaveMinutes > 0),
                TotalWorkingMinutes = attendanceList.Sum(a => a.TotalWorkingMinutes ?? 0),
                OvertimeMinutes = attendanceList.Sum(a => a.OvertimeMinutes ?? 0),
                LateMinutes = attendanceList.Sum(a => a.LateMinutes ?? 0),
                EarlyLeaveMinutes = attendanceList.Sum(a => a.EarlyLeaveMinutes ?? 0)
            };

            return new EmployeeAttendanceHistoryDto
            {
                EmployeeId = employeeId,
                EmployeeName = employee.User.FullName,
                StartDate = startDate,
                EndDate = endDate,
                AttendanceRecords = attendanceList.ToList(),
                Summary = summary
            };
        }

        public async Task<bool> GenerateMonthlyAttendanceSummaryAsync(int year, int month)
        {
            var employees = await _context.Employees.ToListAsync();

            foreach (var employee in employees)
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var attendances = await _context.Attendances
                    .Where(a => a.EmployeeId == employee.Id)
                    .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
                    .ToListAsync();

                var existingSummary = await _context.AttendanceSummaries
                    .FirstOrDefaultAsync(s => s.EmployeeId == employee.Id && s.Year == year && s.Month == month);

                var summary = existingSummary ?? new AttendanceSummary
                {
                    EmployeeId = employee.Id,
                    Year = year,
                    Month = month,
                    CreatedAt = DateTime.UtcNow
                };

                // Calculate statistics
                var workingDaysInMonth = GetWorkingDaysInMonth(year, month);
                
                summary.TotalWorkingDays = workingDaysInMonth;
                summary.ActualWorkingDays = attendances.Count(a => a.CheckInTime != null);
                summary.AbsentDays = workingDaysInMonth - summary.ActualWorkingDays;
                summary.LateDays = attendances.Count(a => a.LateMinutes > 0);
                summary.EarlyLeaveDays = attendances.Count(a => a.EarlyLeaveMinutes > 0);
                summary.TotalWorkingMinutes = attendances.Sum(a => a.TotalWorkingMinutes ?? 0);
                summary.OvertimeMinutes = attendances.Sum(a => a.OvertimeMinutes ?? 0);
                summary.LateMinutes = attendances.Sum(a => a.LateMinutes ?? 0);
                summary.EarlyLeaveMinutes = attendances.Sum(a => a.EarlyLeaveMinutes ?? 0);
                summary.StandardWorkingMinutes = workingDaysInMonth * 8 * 60; // Assuming 8 hours per day
                summary.UpdatedAt = DateTime.UtcNow;

                if (existingSummary == null)
                {
                    _context.AttendanceSummaries.Add(summary);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveMultipleAttendancesAsync(List<int> attendanceIds, int approverId, string? notes = null)
        {
            var attendances = await _context.Attendances
                .Where(a => attendanceIds.Contains(a.Id))
                .ToListAsync();

            foreach (var attendance in attendances)
            {
                attendance.Status = AttendanceStatus.Approved;
                attendance.ManagerNotes = notes;
                attendance.ApprovedById = approverId;
                attendance.ApprovedAt = DateTime.UtcNow;
                attendance.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasCheckedInTodayAsync(int employeeId)
        {
            var today = DateTime.Today;
            return await _context.Attendances
                .AnyAsync(a => a.EmployeeId == employeeId && 
                              a.AttendanceDate.Date == today && 
                              a.CheckInTime != null);
        }

        public async Task<bool> HasCheckedOutTodayAsync(int employeeId)
        {
            var today = DateTime.Today;
            return await _context.Attendances
                .AnyAsync(a => a.EmployeeId == employeeId && 
                              a.AttendanceDate.Date == today && 
                              a.CheckOutTime != null);
        }

        public async Task<AttendanceStatus> CalculateAttendanceStatusAsync(int attendanceId)
        {
            var attendance = await _context.Attendances
                .Include(a => a.WorkShift)
                .FirstOrDefaultAsync(a => a.Id == attendanceId);

            if (attendance == null) return AttendanceStatus.NoShow;

            if (attendance.CheckInTime == null) return AttendanceStatus.NoShow;

            var status = AttendanceStatus.OnTime;

            if (attendance.LateMinutes > 0) status = AttendanceStatus.Late;
            if (attendance.EarlyLeaveMinutes > 0) status = AttendanceStatus.Early;
            if (attendance.OvertimeMinutes > 0 && status == AttendanceStatus.OnTime) status = AttendanceStatus.Overtime;

            return status;
        }

        public async Task<int> CalculateWorkingMinutesAsync(int attendanceId)
        {
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.Id == attendanceId);

            if (attendance?.CheckInTime == null || attendance.CheckOutTime == null)
                return 0;

            var totalMinutes = (int)(attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalMinutes;
            
            if (attendance.BreakMinutes.HasValue)
            {
                totalMinutes -= attendance.BreakMinutes.Value;
            }

            return Math.Max(0, totalMinutes);
        }

        private int GetWorkingDaysInMonth(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var workingDays = 0;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
            }

            return workingDays;
        }
    }
}
