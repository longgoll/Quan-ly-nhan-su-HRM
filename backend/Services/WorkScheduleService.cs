using Microsoft.EntityFrameworkCore;
using AutoMapper;
using backend.Data;
using backend.Models;
using backend.DTOs;

namespace backend.Services
{
    public interface IWorkScheduleService
    {
        // Work Shift Management
        Task<IEnumerable<WorkShiftDto>> GetAllWorkShiftsAsync();
        Task<WorkShiftDto?> GetWorkShiftByIdAsync(int id);
        Task<WorkShiftDto> CreateWorkShiftAsync(CreateWorkShiftDto createDto);
        Task<WorkShiftDto?> UpdateWorkShiftAsync(int id, CreateWorkShiftDto updateDto);
        Task<bool> DeleteWorkShiftAsync(int id);

        // Employee Shift Assignment
        Task<IEnumerable<EmployeeShiftAssignmentDto>> GetEmployeeShiftAssignmentsAsync(int? employeeId = null);
        Task<EmployeeShiftAssignmentDto?> GetEmployeeShiftAssignmentByIdAsync(int id);
        Task<EmployeeShiftAssignmentDto> CreateEmployeeShiftAssignmentAsync(CreateEmployeeShiftAssignmentDto createDto);
        Task<EmployeeShiftAssignmentDto?> UpdateEmployeeShiftAssignmentAsync(int id, CreateEmployeeShiftAssignmentDto updateDto);
        Task<bool> DeleteEmployeeShiftAssignmentAsync(int id);

        // Work Schedule Management
        Task<IEnumerable<WorkScheduleDto>> GetWorkSchedulesAsync(DateTime? startDate = null, DateTime? endDate = null, int? employeeId = null);
        Task<WorkScheduleDto?> GetWorkScheduleByIdAsync(int id);
        Task<WorkScheduleDto> CreateWorkScheduleAsync(CreateWorkScheduleDto createDto);
        Task<IEnumerable<WorkScheduleDto>> CreateBulkWorkScheduleAsync(BulkScheduleCreateDto bulkCreateDto);
        Task<WorkScheduleDto?> UpdateWorkScheduleAsync(int id, CreateWorkScheduleDto updateDto);
        Task<bool> DeleteWorkScheduleAsync(int id);

        // Helper methods
        Task<IEnumerable<WorkScheduleDto>> GetEmployeeScheduleAsync(int employeeId, DateTime startDate, DateTime endDate);
        Task<bool> HasScheduleConflictAsync(int employeeId, DateTime workDate, int? excludeScheduleId = null);
        Task<IEnumerable<WorkShiftDto>> GetApplicableShiftsForEmployeeAsync(int employeeId);
    }

    public class WorkScheduleService : IWorkScheduleService
    {
        private readonly HrmDbContext _context;
        private readonly IMapper _mapper;

        public WorkScheduleService(HrmDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Work Shift Management
        public async Task<IEnumerable<WorkShiftDto>> GetAllWorkShiftsAsync()
        {
            var workShifts = await _context.WorkShifts
                .Where(ws => ws.Status == ShiftStatus.Active)
                .OrderBy(ws => ws.Name)
                .ToListAsync();

            return _mapper.Map<IEnumerable<WorkShiftDto>>(workShifts);
        }

        public async Task<WorkShiftDto?> GetWorkShiftByIdAsync(int id)
        {
            var workShift = await _context.WorkShifts
                .FirstOrDefaultAsync(ws => ws.Id == id);

            return workShift != null ? _mapper.Map<WorkShiftDto>(workShift) : null;
        }

        public async Task<WorkShiftDto> CreateWorkShiftAsync(CreateWorkShiftDto createDto)
        {
            var workShift = _mapper.Map<WorkShift>(createDto);
            workShift.CreatedAt = DateTime.UtcNow;

            _context.WorkShifts.Add(workShift);
            await _context.SaveChangesAsync();

            return _mapper.Map<WorkShiftDto>(workShift);
        }

        public async Task<WorkShiftDto?> UpdateWorkShiftAsync(int id, CreateWorkShiftDto updateDto)
        {
            var workShift = await _context.WorkShifts.FindAsync(id);
            if (workShift == null) return null;

            _mapper.Map(updateDto, workShift);
            workShift.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<WorkShiftDto>(workShift);
        }

        public async Task<bool> DeleteWorkShiftAsync(int id)
        {
            var workShift = await _context.WorkShifts.FindAsync(id);
            if (workShift == null) return false;

            // Check if shift is being used
            var isInUse = await _context.EmployeeShiftAssignments
                .AnyAsync(esa => esa.WorkShiftId == id);

            if (isInUse)
            {
                // Soft delete by changing status
                workShift.Status = ShiftStatus.Inactive;
                workShift.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.WorkShifts.Remove(workShift);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Employee Shift Assignment
        public async Task<IEnumerable<EmployeeShiftAssignmentDto>> GetEmployeeShiftAssignmentsAsync(int? employeeId = null)
        {
            var query = _context.EmployeeShiftAssignments
                .Include(esa => esa.Employee).ThenInclude(e => e.User)
                .Include(esa => esa.WorkShift)
                .AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(esa => esa.EmployeeId == employeeId.Value);
            }

            var assignments = await query
                .OrderBy(esa => esa.Employee.User.FullName)
                .ThenBy(esa => esa.EffectiveFrom)
                .ToListAsync();

            return assignments.Select(esa => new EmployeeShiftAssignmentDto
            {
                Id = esa.Id,
                EmployeeId = esa.EmployeeId,
                EmployeeName = esa.Employee.User.FullName,
                WorkShiftId = esa.WorkShiftId,
                WorkShiftName = esa.WorkShift.Name,
                EffectiveFrom = esa.EffectiveFrom,
                EffectiveTo = esa.EffectiveTo,
                IsDefaultShift = esa.IsDefaultShift,
                RotationOrder = esa.RotationOrder,
                RotationCycleDays = esa.RotationCycleDays,
                Notes = esa.Notes
            });
        }

        public async Task<EmployeeShiftAssignmentDto?> GetEmployeeShiftAssignmentByIdAsync(int id)
        {
            var assignment = await _context.EmployeeShiftAssignments
                .Include(esa => esa.Employee).ThenInclude(e => e.User)
                .Include(esa => esa.WorkShift)
                .FirstOrDefaultAsync(esa => esa.Id == id);

            if (assignment == null) return null;

            return new EmployeeShiftAssignmentDto
            {
                Id = assignment.Id,
                EmployeeId = assignment.EmployeeId,
                EmployeeName = assignment.Employee.User.FullName,
                WorkShiftId = assignment.WorkShiftId,
                WorkShiftName = assignment.WorkShift.Name,
                EffectiveFrom = assignment.EffectiveFrom,
                EffectiveTo = assignment.EffectiveTo,
                IsDefaultShift = assignment.IsDefaultShift,
                RotationOrder = assignment.RotationOrder,
                RotationCycleDays = assignment.RotationCycleDays,
                Notes = assignment.Notes
            };
        }

        public async Task<EmployeeShiftAssignmentDto> CreateEmployeeShiftAssignmentAsync(CreateEmployeeShiftAssignmentDto createDto)
        {
            // Check for overlapping assignments
            var existingAssignment = await _context.EmployeeShiftAssignments
                .Where(esa => esa.EmployeeId == createDto.EmployeeId)
                .Where(esa => esa.EffectiveFrom <= createDto.EffectiveFrom && 
                             (esa.EffectiveTo == null || esa.EffectiveTo >= createDto.EffectiveFrom))
                .FirstOrDefaultAsync();

            if (existingAssignment != null && createDto.EffectiveTo.HasValue)
            {
                // End the existing assignment
                existingAssignment.EffectiveTo = createDto.EffectiveFrom.AddDays(-1);
                existingAssignment.UpdatedAt = DateTime.UtcNow;
            }

            var assignment = _mapper.Map<EmployeeShiftAssignment>(createDto);
            assignment.CreatedAt = DateTime.UtcNow;

            _context.EmployeeShiftAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return await GetEmployeeShiftAssignmentByIdAsync(assignment.Id) ?? 
                   throw new InvalidOperationException("Failed to retrieve created assignment");
        }

        public async Task<EmployeeShiftAssignmentDto?> UpdateEmployeeShiftAssignmentAsync(int id, CreateEmployeeShiftAssignmentDto updateDto)
        {
            var assignment = await _context.EmployeeShiftAssignments.FindAsync(id);
            if (assignment == null) return null;

            _mapper.Map(updateDto, assignment);
            assignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetEmployeeShiftAssignmentByIdAsync(id);
        }

        public async Task<bool> DeleteEmployeeShiftAssignmentAsync(int id)
        {
            var assignment = await _context.EmployeeShiftAssignments.FindAsync(id);
            if (assignment == null) return false;

            _context.EmployeeShiftAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        // Work Schedule Management
        public async Task<IEnumerable<WorkScheduleDto>> GetWorkSchedulesAsync(DateTime? startDate = null, DateTime? endDate = null, int? employeeId = null)
        {
            var query = _context.WorkSchedules
                .Include(ws => ws.Employee).ThenInclude(e => e.User)
                .Include(ws => ws.WorkShift)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(ws => ws.WorkDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ws => ws.WorkDate <= endDate.Value);

            if (employeeId.HasValue)
                query = query.Where(ws => ws.EmployeeId == employeeId.Value);

            var schedules = await query
                .OrderBy(ws => ws.WorkDate)
                .ThenBy(ws => ws.Employee.User.FullName)
                .ToListAsync();

            return schedules.Select(ws => new WorkScheduleDto
            {
                Id = ws.Id,
                EmployeeId = ws.EmployeeId,
                EmployeeName = ws.Employee.User.FullName,
                WorkShiftId = ws.WorkShiftId,
                WorkShiftName = ws.WorkShift.Name,
                WorkDate = ws.WorkDate,
                ActualStartTime = ws.ActualStartTime,
                ActualEndTime = ws.ActualEndTime,
                IsPlanned = ws.IsPlanned,
                ProjectId = ws.ProjectId,
                Notes = ws.Notes
            });
        }

        public async Task<WorkScheduleDto?> GetWorkScheduleByIdAsync(int id)
        {
            var schedule = await _context.WorkSchedules
                .Include(ws => ws.Employee).ThenInclude(e => e.User)
                .Include(ws => ws.WorkShift)
                .FirstOrDefaultAsync(ws => ws.Id == id);

            if (schedule == null) return null;

            return new WorkScheduleDto
            {
                Id = schedule.Id,
                EmployeeId = schedule.EmployeeId,
                EmployeeName = schedule.Employee.User.FullName,
                WorkShiftId = schedule.WorkShiftId,
                WorkShiftName = schedule.WorkShift.Name,
                WorkDate = schedule.WorkDate,
                ActualStartTime = schedule.ActualStartTime,
                ActualEndTime = schedule.ActualEndTime,
                IsPlanned = schedule.IsPlanned,
                ProjectId = schedule.ProjectId,
                Notes = schedule.Notes
            };
        }

        public async Task<WorkScheduleDto> CreateWorkScheduleAsync(CreateWorkScheduleDto createDto)
        {
            // Check for conflicts
            if (await HasScheduleConflictAsync(createDto.EmployeeId, createDto.WorkDate))
            {
                throw new InvalidOperationException("Employee already has a schedule for this date");
            }

            var schedule = _mapper.Map<WorkSchedule>(createDto);
            schedule.CreatedAt = DateTime.UtcNow;

            _context.WorkSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            return await GetWorkScheduleByIdAsync(schedule.Id) ?? 
                   throw new InvalidOperationException("Failed to retrieve created schedule");
        }

        public async Task<IEnumerable<WorkScheduleDto>> CreateBulkWorkScheduleAsync(BulkScheduleCreateDto bulkCreateDto)
        {
            var schedules = new List<WorkSchedule>();
            var currentDate = bulkCreateDto.StartDate.Date;

            while (currentDate <= bulkCreateDto.EndDate.Date)
            {
                // Check if day is selected
                if (bulkCreateDto.SelectedDays.Contains(currentDate.DayOfWeek))
                {
                    // Check if it's a holiday and should be skipped
                    if (bulkCreateDto.SkipHolidays)
                    {
                        var isHoliday = await _context.PublicHolidays
                            .AnyAsync(ph => ph.Date.Date == currentDate && ph.IsActive);
                        
                        if (isHoliday)
                        {
                            currentDate = currentDate.AddDays(1);
                            continue;
                        }
                    }

                    foreach (var employeeId in bulkCreateDto.EmployeeIds)
                    {
                        // Check for conflicts
                        if (!await HasScheduleConflictAsync(employeeId, currentDate))
                        {
                            schedules.Add(new WorkSchedule
                            {
                                EmployeeId = employeeId,
                                WorkShiftId = bulkCreateDto.WorkShiftId,
                                WorkDate = currentDate,
                                IsPlanned = true,
                                Notes = bulkCreateDto.Notes,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            _context.WorkSchedules.AddRange(schedules);
            await _context.SaveChangesAsync();

            var scheduleIds = schedules.Select(s => s.Id).ToList();
            return await GetWorkSchedulesAsync(bulkCreateDto.StartDate, bulkCreateDto.EndDate);
        }

        public async Task<WorkScheduleDto?> UpdateWorkScheduleAsync(int id, CreateWorkScheduleDto updateDto)
        {
            var schedule = await _context.WorkSchedules.FindAsync(id);
            if (schedule == null) return null;

            _mapper.Map(updateDto, schedule);
            schedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetWorkScheduleByIdAsync(id);
        }

        public async Task<bool> DeleteWorkScheduleAsync(int id)
        {
            var schedule = await _context.WorkSchedules.FindAsync(id);
            if (schedule == null) return false;

            _context.WorkSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return true;
        }

        // Helper methods
        public async Task<IEnumerable<WorkScheduleDto>> GetEmployeeScheduleAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            return await GetWorkSchedulesAsync(startDate, endDate, employeeId);
        }

        public async Task<bool> HasScheduleConflictAsync(int employeeId, DateTime workDate, int? excludeScheduleId = null)
        {
            var query = _context.WorkSchedules
                .Where(ws => ws.EmployeeId == employeeId && ws.WorkDate.Date == workDate.Date);

            if (excludeScheduleId.HasValue)
            {
                query = query.Where(ws => ws.Id != excludeScheduleId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<WorkShiftDto>> GetApplicableShiftsForEmployeeAsync(int employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null) return new List<WorkShiftDto>();

            var workShifts = await _context.WorkShifts
                .Where(ws => ws.Status == ShiftStatus.Active)
                .ToListAsync();

            return _mapper.Map<IEnumerable<WorkShiftDto>>(workShifts);
        }
    }
}
