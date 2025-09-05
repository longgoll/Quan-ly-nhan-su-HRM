using Microsoft.EntityFrameworkCore;
using AutoMapper;
using backend.Data;
using backend.Models;
using backend.DTOs;

namespace backend.Services
{
    public interface ILeaveService
    {
        // Leave Policy Management
        Task<IEnumerable<LeavePolicyDto>> GetAllLeavePoliciesAsync();
        Task<LeavePolicyDto?> GetLeavePolicyByIdAsync(int id);
        Task<LeavePolicyDto> CreateLeavePolicyAsync(CreateLeavePolicyDto createDto);
        Task<LeavePolicyDto?> UpdateLeavePolicyAsync(int id, CreateLeavePolicyDto updateDto);
        Task<bool> DeleteLeavePolicyAsync(int id);

        // Employee Leave Balance Management
        Task<IEnumerable<EmployeeLeaveBalanceDto>> GetEmployeeLeaveBalancesAsync(int employeeId, int? year = null);
        Task<EmployeeLeaveBalanceDto?> GetLeaveBalanceAsync(int employeeId, int leavePolicyId, int year);
        Task<bool> InitializeLeaveBalancesForYearAsync(int year);
        Task<bool> AdjustLeaveBalanceAsync(LeaveBalanceAdjustmentDto adjustmentDto);

        // Leave Request Management
        Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsAsync(LeaveReportFilterDto filter);
        Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(int id);
        Task<LeaveRequestDto> CreateLeaveRequestAsync(int employeeId, CreateLeaveRequestDto createDto);
        Task<LeaveRequestDto?> UpdateLeaveRequestAsync(int id, CreateLeaveRequestDto updateDto);
        Task<bool> CancelLeaveRequestAsync(int id, int employeeId);
        Task<bool> DeleteLeaveRequestAsync(int id);

        // Leave Approval Workflow
        Task<LeaveRequestDto> ProcessLeaveApprovalAsync(int approverId, ProcessLeaveApprovalDto approvalDto);
        Task<IEnumerable<LeaveRequestDto>> GetPendingApprovalsAsync(int approverId);
        Task<bool> SetupApprovalWorkflowAsync(int leaveRequestId, List<int> approverIds);

        // Public Holiday Management
        Task<IEnumerable<PublicHolidayDto>> GetPublicHolidaysAsync(int? year = null, int? departmentId = null);
        Task<PublicHolidayDto?> GetPublicHolidayByIdAsync(int id);
        Task<PublicHolidayDto> CreatePublicHolidayAsync(CreatePublicHolidayDto createDto);
        Task<PublicHolidayDto?> UpdatePublicHolidayAsync(int id, CreatePublicHolidayDto updateDto);
        Task<bool> DeletePublicHolidayAsync(int id);

        // Reports and Analytics
        Task<IEnumerable<LeaveCalendarDto>> GetLeaveCalendarAsync(DateTime startDate, DateTime endDate, int? departmentId = null);
        Task<EmployeeLeaveHistoryDto> GetEmployeeLeaveHistoryAsync(int employeeId, int year);
        Task<IEnumerable<EmployeeLeaveBalanceDto>> GetDepartmentLeaveBalancesAsync(int departmentId, int year);

        // Helper Methods
        Task<bool> CanRequestLeaveAsync(int employeeId, int leavePolicyId, DateTime startDate, DateTime endDate);
        Task<decimal> CalculateRequestedDaysAsync(DateTime startDate, DateTime endDate, bool includeWeekends = false);
        Task<bool> HasLeaveConflictAsync(int employeeId, DateTime startDate, DateTime endDate, int? excludeRequestId = null);
        Task<IEnumerable<LeavePolicyDto>> GetApplicableLeavePoliciesAsync(int employeeId);
    }

    public class LeaveService : ILeaveService
    {
        private readonly HrmDbContext _context;
        private readonly IMapper _mapper;

        public LeaveService(HrmDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Leave Policy Management
        public async Task<IEnumerable<LeavePolicyDto>> GetAllLeavePoliciesAsync()
        {
            var policies = await _context.LeavePolicies
                .Include(lp => lp.Department)
                .Include(lp => lp.Position)
                .Where(lp => lp.IsActive)
                .OrderBy(lp => lp.LeaveType)
                .ThenBy(lp => lp.Name)
                .ToListAsync();

            return policies.Select(lp => new LeavePolicyDto
            {
                Id = lp.Id,
                Name = lp.Name,
                Description = lp.Description,
                LeaveType = lp.LeaveType,
                AnnualAllowanceDays = lp.AnnualAllowanceDays,
                MaxCarryForwardDays = lp.MaxCarryForwardDays,
                MaxConsecutiveDays = lp.MaxConsecutiveDays,
                MinAdvanceNoticeDays = lp.MinAdvanceNoticeDays,
                RequiresDocumentation = lp.RequiresDocumentation,
                IsPaid = lp.IsPaid,
                DepartmentId = lp.DepartmentId,
                DepartmentName = lp.Department?.Name,
                PositionId = lp.PositionId,
                PositionName = lp.Position?.Title,
                MinTenureMonths = lp.MinTenureMonths,
                IsActive = lp.IsActive,
                EffectiveFrom = lp.EffectiveFrom,
                EffectiveTo = lp.EffectiveTo
            });
        }

        public async Task<LeavePolicyDto?> GetLeavePolicyByIdAsync(int id)
        {
            var policy = await _context.LeavePolicies
                .Include(lp => lp.Department)
                .Include(lp => lp.Position)
                .FirstOrDefaultAsync(lp => lp.Id == id);

            if (policy == null) return null;

            return new LeavePolicyDto
            {
                Id = policy.Id,
                Name = policy.Name,
                Description = policy.Description,
                LeaveType = policy.LeaveType,
                AnnualAllowanceDays = policy.AnnualAllowanceDays,
                MaxCarryForwardDays = policy.MaxCarryForwardDays,
                MaxConsecutiveDays = policy.MaxConsecutiveDays,
                MinAdvanceNoticeDays = policy.MinAdvanceNoticeDays,
                RequiresDocumentation = policy.RequiresDocumentation,
                IsPaid = policy.IsPaid,
                DepartmentId = policy.DepartmentId,
                DepartmentName = policy.Department?.Name,
                PositionId = policy.PositionId,
                PositionName = policy.Position?.Title,
                MinTenureMonths = policy.MinTenureMonths,
                IsActive = policy.IsActive,
                EffectiveFrom = policy.EffectiveFrom,
                EffectiveTo = policy.EffectiveTo
            };
        }

        public async Task<LeavePolicyDto> CreateLeavePolicyAsync(CreateLeavePolicyDto createDto)
        {
            var policy = _mapper.Map<LeavePolicy>(createDto);
            policy.IsActive = true;
            policy.CreatedAt = DateTime.UtcNow;

            _context.LeavePolicies.Add(policy);
            await _context.SaveChangesAsync();

            return await GetLeavePolicyByIdAsync(policy.Id) ?? 
                   throw new InvalidOperationException("Failed to retrieve created policy");
        }

        public async Task<LeavePolicyDto?> UpdateLeavePolicyAsync(int id, CreateLeavePolicyDto updateDto)
        {
            var policy = await _context.LeavePolicies.FindAsync(id);
            if (policy == null) return null;

            _mapper.Map(updateDto, policy);
            policy.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetLeavePolicyByIdAsync(id);
        }

        public async Task<bool> DeleteLeavePolicyAsync(int id)
        {
            var policy = await _context.LeavePolicies.FindAsync(id);
            if (policy == null) return false;

            // Check if policy is being used
            var isInUse = await _context.EmployeeLeaveBalances
                .AnyAsync(elb => elb.LeavePolicyId == id);

            if (isInUse)
            {
                // Soft delete
                policy.IsActive = false;
                policy.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.LeavePolicies.Remove(policy);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Employee Leave Balance Management
        public async Task<IEnumerable<EmployeeLeaveBalanceDto>> GetEmployeeLeaveBalancesAsync(int employeeId, int? year = null)
        {
            var currentYear = year ?? DateTime.Now.Year;

            var balances = await _context.EmployeeLeaveBalances
                .Include(elb => elb.Employee).ThenInclude(e => e.User)
                .Include(elb => elb.LeavePolicy)
                .Where(elb => elb.EmployeeId == employeeId && elb.Year == currentYear)
                .ToListAsync();

            return balances.Select(elb => new EmployeeLeaveBalanceDto
            {
                Id = elb.Id,
                EmployeeId = elb.EmployeeId,
                EmployeeName = elb.Employee.User.FullName,
                LeavePolicyId = elb.LeavePolicyId,
                LeavePolicyName = elb.LeavePolicy.Name,
                LeaveType = elb.LeavePolicy.LeaveType,
                Year = elb.Year,
                AllocatedDays = elb.AllocatedDays,
                UsedDays = elb.UsedDays,
                CarriedForwardDays = elb.CarriedForwardDays,
                AdjustmentDays = elb.AdjustmentDays,
                RemainingDays = elb.RemainingDays
            });
        }

        public async Task<EmployeeLeaveBalanceDto?> GetLeaveBalanceAsync(int employeeId, int leavePolicyId, int year)
        {
            var balance = await _context.EmployeeLeaveBalances
                .Include(elb => elb.Employee).ThenInclude(e => e.User)
                .Include(elb => elb.LeavePolicy)
                .FirstOrDefaultAsync(elb => elb.EmployeeId == employeeId && 
                                          elb.LeavePolicyId == leavePolicyId && 
                                          elb.Year == year);

            if (balance == null) return null;

            return new EmployeeLeaveBalanceDto
            {
                Id = balance.Id,
                EmployeeId = balance.EmployeeId,
                EmployeeName = balance.Employee.User.FullName,
                LeavePolicyId = balance.LeavePolicyId,
                LeavePolicyName = balance.LeavePolicy.Name,
                LeaveType = balance.LeavePolicy.LeaveType,
                Year = balance.Year,
                AllocatedDays = balance.AllocatedDays,
                UsedDays = balance.UsedDays,
                CarriedForwardDays = balance.CarriedForwardDays,
                AdjustmentDays = balance.AdjustmentDays,
                RemainingDays = balance.RemainingDays
            };
        }

        public async Task<bool> InitializeLeaveBalancesForYearAsync(int year)
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Where(e => e.Status == EmployeeStatus.Active)
                .ToListAsync();

            var policies = await _context.LeavePolicies
                .Where(lp => lp.IsActive)
                .Where(lp => lp.EffectiveFrom <= new DateTime(year, 12, 31))
                .Where(lp => lp.EffectiveTo == null || lp.EffectiveTo >= new DateTime(year, 1, 1))
                .ToListAsync();

            foreach (var employee in employees)
            {
                // Calculate tenure in months
                var tenureMonths = ((year - employee.HireDate.Year) * 12) + (12 - employee.HireDate.Month + 1);

                foreach (var policy in policies)
                {
                    // Check if policy applies to this employee
                    if (policy.DepartmentId.HasValue && policy.DepartmentId != employee.DepartmentId) continue;
                    if (policy.PositionId.HasValue && policy.PositionId != employee.PositionId) continue;
                    if (tenureMonths < policy.MinTenureMonths) continue;

                    // Check if balance already exists
                    var existingBalance = await _context.EmployeeLeaveBalances
                        .FirstOrDefaultAsync(elb => elb.EmployeeId == employee.Id && 
                                                   elb.LeavePolicyId == policy.Id && 
                                                   elb.Year == year);

                    if (existingBalance != null) continue;

                    // Get carry forward from previous year
                    var previousYearBalance = await _context.EmployeeLeaveBalances
                        .FirstOrDefaultAsync(elb => elb.EmployeeId == employee.Id && 
                                                   elb.LeavePolicyId == policy.Id && 
                                                   elb.Year == year - 1);

                    var carryForward = 0m;
                    if (previousYearBalance != null)
                    {
                        var remainingDays = previousYearBalance.RemainingDays;
                        carryForward = Math.Min(remainingDays, policy.MaxCarryForwardDays);
                    }

                    // Create new balance
                    var newBalance = new EmployeeLeaveBalance
                    {
                        EmployeeId = employee.Id,
                        LeavePolicyId = policy.Id,
                        Year = year,
                        AllocatedDays = policy.AnnualAllowanceDays,
                        UsedDays = 0,
                        CarriedForwardDays = carryForward,
                        AdjustmentDays = 0,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.EmployeeLeaveBalances.Add(newBalance);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AdjustLeaveBalanceAsync(LeaveBalanceAdjustmentDto adjustmentDto)
        {
            var balance = await _context.EmployeeLeaveBalances
                .FirstOrDefaultAsync(elb => elb.EmployeeId == adjustmentDto.EmployeeId && 
                                          elb.LeavePolicyId == adjustmentDto.LeavePolicyId && 
                                          elb.Year == adjustmentDto.Year);

            if (balance == null)
            {
                throw new ArgumentException("Leave balance not found");
            }

            balance.AdjustmentDays += adjustmentDto.AdjustmentDays;
            balance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // Leave Request Management
        public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsAsync(LeaveReportFilterDto filter)
        {
            var query = _context.LeaveRequests
                .Include(lr => lr.Employee).ThenInclude(e => e.User)
                .Include(lr => lr.LeavePolicy)
                .Include(lr => lr.CoverEmployee).ThenInclude(ce => ce!.User)
                .Include(lr => lr.ApprovedBy).ThenInclude(ab => ab!.User)
                .Include(lr => lr.LeaveApprovalWorkflows).ThenInclude(law => law.ApproverEmployee).ThenInclude(ae => ae.User)
                .AsQueryable();

            if (filter.EmployeeId.HasValue)
                query = query.Where(lr => lr.EmployeeId == filter.EmployeeId.Value);

            if (filter.DepartmentId.HasValue)
                query = query.Where(lr => lr.Employee.DepartmentId == filter.DepartmentId.Value);

            if (filter.LeaveType.HasValue)
                query = query.Where(lr => lr.LeavePolicy.LeaveType == filter.LeaveType.Value);

            if (filter.Status.HasValue)
                query = query.Where(lr => lr.Status == filter.Status.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(lr => lr.EndDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(lr => lr.StartDate <= filter.EndDate.Value);

            if (filter.Year.HasValue)
                query = query.Where(lr => lr.StartDate.Year == filter.Year.Value || lr.EndDate.Year == filter.Year.Value);

            if (filter.Month.HasValue)
                query = query.Where(lr => lr.StartDate.Month == filter.Month.Value || lr.EndDate.Month == filter.Month.Value);

            var requests = await query
                .OrderByDescending(lr => lr.CreatedAt)
                .ToListAsync();

            return requests.Select(lr => new LeaveRequestDto
            {
                Id = lr.Id,
                EmployeeId = lr.EmployeeId,
                EmployeeName = lr.Employee.User.FullName,
                LeavePolicyId = lr.LeavePolicyId,
                LeavePolicyName = lr.LeavePolicy.Name,
                LeaveType = lr.LeavePolicy.LeaveType,
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                RequestedDays = lr.RequestedDays,
                Reason = lr.Reason,
                AttachmentUrl = lr.AttachmentUrl,
                AttachmentFileName = lr.AttachmentFileName,
                CoverEmployeeId = lr.CoverEmployeeId,
                CoverEmployeeName = lr.CoverEmployee?.User?.FullName,
                CoverNotes = lr.CoverNotes,
                Status = lr.Status,
                ManagerComments = lr.ManagerComments,
                ApprovedByName = lr.ApprovedBy?.User?.FullName,
                ApprovedAt = lr.ApprovedAt,
                EmergencyContactPhone = lr.EmergencyContactPhone,
                EmergencyContactAddress = lr.EmergencyContactAddress,
                CreatedAt = lr.CreatedAt,
                ApprovalWorkflow = lr.LeaveApprovalWorkflows.Select(law => new LeaveApprovalWorkflowDto
                {
                    Id = law.Id,
                    LeaveRequestId = law.LeaveRequestId,
                    ApproverEmployeeId = law.ApproverEmployeeId,
                    ApproverEmployeeName = law.ApproverEmployee.User.FullName,
                    Order = law.Order,
                    Status = law.Status,
                    Comments = law.Comments,
                    ProcessedAt = law.ProcessedAt
                }).OrderBy(law => law.Order).ToList()
            });
        }

        public async Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(int id)
        {
            var request = await _context.LeaveRequests
                .Include(lr => lr.Employee).ThenInclude(e => e.User)
                .Include(lr => lr.LeavePolicy)
                .Include(lr => lr.CoverEmployee).ThenInclude(ce => ce!.User)
                .Include(lr => lr.ApprovedBy).ThenInclude(ab => ab!.User)
                .Include(lr => lr.LeaveApprovalWorkflows).ThenInclude(law => law.ApproverEmployee).ThenInclude(ae => ae.User)
                .FirstOrDefaultAsync(lr => lr.Id == id);

            if (request == null) return null;

            return new LeaveRequestDto
            {
                Id = request.Id,
                EmployeeId = request.EmployeeId,
                EmployeeName = request.Employee.User.FullName,
                LeavePolicyId = request.LeavePolicyId,
                LeavePolicyName = request.LeavePolicy.Name,
                LeaveType = request.LeavePolicy.LeaveType,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RequestedDays = request.RequestedDays,
                Reason = request.Reason,
                AttachmentUrl = request.AttachmentUrl,
                AttachmentFileName = request.AttachmentFileName,
                CoverEmployeeId = request.CoverEmployeeId,
                CoverEmployeeName = request.CoverEmployee?.User?.FullName,
                CoverNotes = request.CoverNotes,
                Status = request.Status,
                ManagerComments = request.ManagerComments,
                ApprovedByName = request.ApprovedBy?.User?.FullName,
                ApprovedAt = request.ApprovedAt,
                EmergencyContactPhone = request.EmergencyContactPhone,
                EmergencyContactAddress = request.EmergencyContactAddress,
                CreatedAt = request.CreatedAt,
                ApprovalWorkflow = request.LeaveApprovalWorkflows.Select(law => new LeaveApprovalWorkflowDto
                {
                    Id = law.Id,
                    LeaveRequestId = law.LeaveRequestId,
                    ApproverEmployeeId = law.ApproverEmployeeId,
                    ApproverEmployeeName = law.ApproverEmployee.User.FullName,
                    Order = law.Order,
                    Status = law.Status,
                    Comments = law.Comments,
                    ProcessedAt = law.ProcessedAt
                }).OrderBy(law => law.Order).ToList()
            };
        }

        public async Task<LeaveRequestDto> CreateLeaveRequestAsync(int employeeId, CreateLeaveRequestDto createDto)
        {
            // Validate request
            if (!await CanRequestLeaveAsync(employeeId, createDto.LeavePolicyId, createDto.StartDate, createDto.EndDate))
            {
                throw new InvalidOperationException("Leave request cannot be processed. Check leave balance and policies.");
            }

            // Check for conflicts
            if (await HasLeaveConflictAsync(employeeId, createDto.StartDate, createDto.EndDate))
            {
                throw new InvalidOperationException("Leave request conflicts with existing leave request.");
            }

            // Calculate requested days
            var requestedDays = await CalculateRequestedDaysAsync(createDto.StartDate, createDto.EndDate);

            var leaveRequest = _mapper.Map<LeaveRequest>(createDto);
            leaveRequest.EmployeeId = employeeId;
            leaveRequest.RequestedDays = requestedDays;
            leaveRequest.Status = LeaveStatus.Pending;
            leaveRequest.CreatedAt = DateTime.UtcNow;

            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            // Setup approval workflow
            var employee = await _context.Employees
                .Include(e => e.DirectManager)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee?.DirectManagerId.HasValue == true)
            {
                await SetupApprovalWorkflowAsync(leaveRequest.Id, new List<int> { employee.DirectManagerId.Value });
            }

            return await GetLeaveRequestByIdAsync(leaveRequest.Id) ?? 
                   throw new InvalidOperationException("Failed to retrieve created leave request");
        }

        public async Task<LeaveRequestDto?> UpdateLeaveRequestAsync(int id, CreateLeaveRequestDto updateDto)
        {
            var request = await _context.LeaveRequests.FindAsync(id);
            if (request == null) return null;

            if (request.Status != LeaveStatus.Pending)
            {
                throw new InvalidOperationException("Cannot update leave request that is not pending");
            }

            // Recalculate requested days
            var requestedDays = await CalculateRequestedDaysAsync(updateDto.StartDate, updateDto.EndDate);

            _mapper.Map(updateDto, request);
            request.RequestedDays = requestedDays;
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetLeaveRequestByIdAsync(id);
        }

        public async Task<bool> CancelLeaveRequestAsync(int id, int employeeId)
        {
            var request = await _context.LeaveRequests
                .FirstOrDefaultAsync(lr => lr.Id == id && lr.EmployeeId == employeeId);

            if (request == null) return false;

            if (request.Status == LeaveStatus.Completed)
            {
                throw new InvalidOperationException("Cannot cancel completed leave request");
            }

            request.Status = LeaveStatus.Cancelled;
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteLeaveRequestAsync(int id)
        {
            var request = await _context.LeaveRequests.FindAsync(id);
            if (request == null) return false;

            if (request.Status == LeaveStatus.Approved || request.Status == LeaveStatus.InProgress || request.Status == LeaveStatus.Completed)
            {
                throw new InvalidOperationException("Cannot delete approved or active leave request");
            }

            _context.LeaveRequests.Remove(request);
            await _context.SaveChangesAsync();
            return true;
        }

        // Leave Approval Workflow
        public async Task<LeaveRequestDto> ProcessLeaveApprovalAsync(int approverId, ProcessLeaveApprovalDto approvalDto)
        {
            var workflow = await _context.LeaveApprovalWorkflows
                .Include(law => law.LeaveRequest)
                .FirstOrDefaultAsync(law => law.LeaveRequestId == approvalDto.LeaveRequestId && 
                                          law.ApproverEmployeeId == approverId && 
                                          law.Status == LeaveStatus.Pending);

            if (workflow == null)
            {
                throw new ArgumentException("Approval workflow not found or already processed");
            }

            workflow.Status = approvalDto.Status;
            workflow.Comments = approvalDto.Comments;
            workflow.ProcessedAt = DateTime.UtcNow;

            var leaveRequest = workflow.LeaveRequest;

            if (approvalDto.Status == LeaveStatus.Rejected)
            {
                leaveRequest.Status = LeaveStatus.Rejected;
                leaveRequest.ManagerComments = approvalDto.Comments;
            }
            else if (approvalDto.Status == LeaveStatus.Approved)
            {
                // Check if there are more approvers in the workflow
                var nextWorkflow = await _context.LeaveApprovalWorkflows
                    .Where(law => law.LeaveRequestId == approvalDto.LeaveRequestId)
                    .Where(law => law.Order > workflow.Order)
                    .Where(law => law.Status == LeaveStatus.Pending)
                    .OrderBy(law => law.Order)
                    .FirstOrDefaultAsync();

                if (nextWorkflow == null)
                {
                    // Final approval
                    leaveRequest.Status = LeaveStatus.Approved;
                    leaveRequest.ApprovedById = approverId;
                    leaveRequest.ApprovedAt = DateTime.UtcNow;

                    // Update leave balance
                    await UpdateLeaveBalanceForApprovedRequestAsync(leaveRequest);
                }
                // else: leave status remains pending for next approver
            }

            leaveRequest.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetLeaveRequestByIdAsync(leaveRequest.Id) ?? 
                   throw new InvalidOperationException("Failed to retrieve leave request");
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetPendingApprovalsAsync(int approverId)
        {
            var workflowItems = await _context.LeaveApprovalWorkflows
                .Include(law => law.LeaveRequest).ThenInclude(lr => lr.Employee).ThenInclude(e => e.User)
                .Include(law => law.LeaveRequest).ThenInclude(lr => lr.LeavePolicy)
                .Where(law => law.ApproverEmployeeId == approverId && law.Status == LeaveStatus.Pending)
                .Where(law => law.LeaveRequest.Status == LeaveStatus.Pending)
                .ToListAsync();

            var requestIds = workflowItems.Select(wi => wi.LeaveRequestId).ToList();

            return await GetLeaveRequestsAsync(new LeaveReportFilterDto
            {
                Status = LeaveStatus.Pending
            });
        }

        public async Task<bool> SetupApprovalWorkflowAsync(int leaveRequestId, List<int> approverIds)
        {
            // Remove existing workflow
            var existingWorkflow = await _context.LeaveApprovalWorkflows
                .Where(law => law.LeaveRequestId == leaveRequestId)
                .ToListAsync();

            _context.LeaveApprovalWorkflows.RemoveRange(existingWorkflow);

            // Create new workflow
            for (int i = 0; i < approverIds.Count; i++)
            {
                var workflowItem = new LeaveApprovalWorkflow
                {
                    LeaveRequestId = leaveRequestId,
                    ApproverEmployeeId = approverIds[i],
                    Order = i + 1,
                    Status = LeaveStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.LeaveApprovalWorkflows.Add(workflowItem);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Public Holiday Management
        public async Task<IEnumerable<PublicHolidayDto>> GetPublicHolidaysAsync(int? year = null, int? departmentId = null)
        {
            var query = _context.PublicHolidays
                .Include(ph => ph.Department)
                .Where(ph => ph.IsActive)
                .AsQueryable();

            if (year.HasValue)
                query = query.Where(ph => ph.Date.Year == year.Value);

            if (departmentId.HasValue)
                query = query.Where(ph => ph.DepartmentId == null || ph.DepartmentId == departmentId.Value);

            var holidays = await query
                .OrderBy(ph => ph.Date)
                .ToListAsync();

            return holidays.Select(ph => new PublicHolidayDto
            {
                Id = ph.Id,
                Name = ph.Name,
                Description = ph.Description,
                Date = ph.Date,
                IsPaid = ph.IsPaid,
                IsMandatory = ph.IsMandatory,
                DepartmentId = ph.DepartmentId,
                DepartmentName = ph.Department?.Name,
                IsActive = ph.IsActive
            });
        }

        public async Task<PublicHolidayDto?> GetPublicHolidayByIdAsync(int id)
        {
            var holiday = await _context.PublicHolidays
                .Include(ph => ph.Department)
                .FirstOrDefaultAsync(ph => ph.Id == id);

            if (holiday == null) return null;

            return new PublicHolidayDto
            {
                Id = holiday.Id,
                Name = holiday.Name,
                Description = holiday.Description,
                Date = holiday.Date,
                IsPaid = holiday.IsPaid,
                IsMandatory = holiday.IsMandatory,
                DepartmentId = holiday.DepartmentId,
                DepartmentName = holiday.Department?.Name,
                IsActive = holiday.IsActive
            };
        }

        public async Task<PublicHolidayDto> CreatePublicHolidayAsync(CreatePublicHolidayDto createDto)
        {
            var holiday = _mapper.Map<PublicHoliday>(createDto);
            holiday.IsActive = true;
            holiday.CreatedAt = DateTime.UtcNow;

            _context.PublicHolidays.Add(holiday);
            await _context.SaveChangesAsync();

            return await GetPublicHolidayByIdAsync(holiday.Id) ?? 
                   throw new InvalidOperationException("Failed to retrieve created holiday");
        }

        public async Task<PublicHolidayDto?> UpdatePublicHolidayAsync(int id, CreatePublicHolidayDto updateDto)
        {
            var holiday = await _context.PublicHolidays.FindAsync(id);
            if (holiday == null) return null;

            _mapper.Map(updateDto, holiday);
            holiday.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetPublicHolidayByIdAsync(id);
        }

        public async Task<bool> DeletePublicHolidayAsync(int id)
        {
            var holiday = await _context.PublicHolidays.FindAsync(id);
            if (holiday == null) return false;

            _context.PublicHolidays.Remove(holiday);
            await _context.SaveChangesAsync();
            return true;
        }

        // Reports and Analytics
        public async Task<IEnumerable<LeaveCalendarDto>> GetLeaveCalendarAsync(DateTime startDate, DateTime endDate, int? departmentId = null)
        {
            var leaveRequests = await GetLeaveRequestsAsync(new LeaveReportFilterDto
            {
                StartDate = startDate,
                EndDate = endDate,
                DepartmentId = departmentId,
                Status = LeaveStatus.Approved
            });

            var holidays = await GetPublicHolidaysAsync(startDate.Year, departmentId);

            var calendar = new List<LeaveCalendarDto>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var dayHoliday = holidays.FirstOrDefault(h => h.Date.Date == currentDate);
                var dayLeaves = leaveRequests.Where(lr => lr.StartDate.Date <= currentDate && lr.EndDate.Date >= currentDate);

                calendar.Add(new LeaveCalendarDto
                {
                    Date = currentDate,
                    IsPublicHoliday = dayHoliday != null,
                    PublicHolidayName = dayHoliday?.Name,
                    Entries = dayLeaves.Select(lr => new LeaveCalendarEntryDto
                    {
                        LeaveRequestId = lr.Id,
                        EmployeeId = lr.EmployeeId,
                        EmployeeName = lr.EmployeeName,
                        LeaveType = lr.LeaveType,
                        Status = lr.Status,
                        IsStartDate = lr.StartDate.Date == currentDate,
                        IsEndDate = lr.EndDate.Date == currentDate,
                        RequestedDays = lr.RequestedDays
                    }).ToList()
                });

                currentDate = currentDate.AddDays(1);
            }

            return calendar;
        }

        public async Task<EmployeeLeaveHistoryDto> GetEmployeeLeaveHistoryAsync(int employeeId, int year)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
            {
                throw new ArgumentException("Employee not found", nameof(employeeId));
            }

            var balances = await GetEmployeeLeaveBalancesAsync(employeeId, year);
            var requests = await GetLeaveRequestsAsync(new LeaveReportFilterDto
            {
                EmployeeId = employeeId,
                Year = year
            });

            return new EmployeeLeaveHistoryDto
            {
                EmployeeId = employeeId,
                EmployeeName = employee.User.FullName,
                Year = year,
                LeaveBalances = balances.ToList(),
                LeaveRequests = requests.ToList(),
                TotalLeaveDaysTaken = requests.Where(lr => lr.Status == LeaveStatus.Approved || lr.Status == LeaveStatus.Completed)
                                            .Sum(lr => lr.RequestedDays),
                TotalLeaveBalance = balances.Sum(lb => lb.RemainingDays)
            };
        }

        public async Task<IEnumerable<EmployeeLeaveBalanceDto>> GetDepartmentLeaveBalancesAsync(int departmentId, int year)
        {
            var balances = await _context.EmployeeLeaveBalances
                .Include(elb => elb.Employee).ThenInclude(e => e.User)
                .Include(elb => elb.LeavePolicy)
                .Where(elb => elb.Employee.DepartmentId == departmentId && elb.Year == year)
                .ToListAsync();

            return balances.Select(elb => new EmployeeLeaveBalanceDto
            {
                Id = elb.Id,
                EmployeeId = elb.EmployeeId,
                EmployeeName = elb.Employee.User.FullName,
                LeavePolicyId = elb.LeavePolicyId,
                LeavePolicyName = elb.LeavePolicy.Name,
                LeaveType = elb.LeavePolicy.LeaveType,
                Year = elb.Year,
                AllocatedDays = elb.AllocatedDays,
                UsedDays = elb.UsedDays,
                CarriedForwardDays = elb.CarriedForwardDays,
                AdjustmentDays = elb.AdjustmentDays,
                RemainingDays = elb.RemainingDays
            });
        }

        // Helper Methods
        public async Task<bool> CanRequestLeaveAsync(int employeeId, int leavePolicyId, DateTime startDate, DateTime endDate)
        {
            var policy = await _context.LeavePolicies.FindAsync(leavePolicyId);
            if (policy == null || !policy.IsActive) return false;

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return false;

            // Check minimum advance notice
            var daysUntilStart = (startDate.Date - DateTime.Today).Days;
            if (daysUntilStart < policy.MinAdvanceNoticeDays) return false;

            // Check maximum consecutive days
            var requestedDays = await CalculateRequestedDaysAsync(startDate, endDate);
            if (requestedDays > policy.MaxConsecutiveDays) return false;

            // Check leave balance
            var balance = await GetLeaveBalanceAsync(employeeId, leavePolicyId, startDate.Year);
            if (balance == null || balance.RemainingDays < requestedDays) return false;

            return true;
        }

        public async Task<decimal> CalculateRequestedDaysAsync(DateTime startDate, DateTime endDate, bool includeWeekends = false)
        {
            if (startDate > endDate) return 0;

            var days = 0m;
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                if (includeWeekends || (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday))
                {
                    // Check if it's a public holiday
                    var isHoliday = await _context.PublicHolidays
                        .AnyAsync(ph => ph.Date.Date == currentDate && ph.IsActive);

                    if (!isHoliday)
                    {
                        days += 1;
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            return days;
        }

        public async Task<bool> HasLeaveConflictAsync(int employeeId, DateTime startDate, DateTime endDate, int? excludeRequestId = null)
        {
            var query = _context.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId)
                .Where(lr => lr.Status == LeaveStatus.Approved || lr.Status == LeaveStatus.Pending || lr.Status == LeaveStatus.InProgress)
                .Where(lr => !(lr.EndDate < startDate || lr.StartDate > endDate));

            if (excludeRequestId.HasValue)
            {
                query = query.Where(lr => lr.Id != excludeRequestId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<LeavePolicyDto>> GetApplicableLeavePoliciesAsync(int employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null) return new List<LeavePolicyDto>();

            var tenureMonths = ((DateTime.Now.Year - employee.HireDate.Year) * 12) + (DateTime.Now.Month - employee.HireDate.Month);

            var policies = await _context.LeavePolicies
                .Include(lp => lp.Department)
                .Include(lp => lp.Position)
                .Where(lp => lp.IsActive)
                .Where(lp => lp.EffectiveFrom <= DateTime.Now)
                .Where(lp => lp.EffectiveTo == null || lp.EffectiveTo >= DateTime.Now)
                .Where(lp => lp.DepartmentId == null || lp.DepartmentId == employee.DepartmentId)
                .Where(lp => lp.PositionId == null || lp.PositionId == employee.PositionId)
                .Where(lp => lp.MinTenureMonths <= tenureMonths)
                .ToListAsync();

            return policies.Select(lp => new LeavePolicyDto
            {
                Id = lp.Id,
                Name = lp.Name,
                Description = lp.Description,
                LeaveType = lp.LeaveType,
                AnnualAllowanceDays = lp.AnnualAllowanceDays,
                MaxCarryForwardDays = lp.MaxCarryForwardDays,
                MaxConsecutiveDays = lp.MaxConsecutiveDays,
                MinAdvanceNoticeDays = lp.MinAdvanceNoticeDays,
                RequiresDocumentation = lp.RequiresDocumentation,
                IsPaid = lp.IsPaid,
                DepartmentId = lp.DepartmentId,
                DepartmentName = lp.Department?.Name,
                PositionId = lp.PositionId,
                PositionName = lp.Position?.Title,
                MinTenureMonths = lp.MinTenureMonths,
                IsActive = lp.IsActive,
                EffectiveFrom = lp.EffectiveFrom,
                EffectiveTo = lp.EffectiveTo
            });
        }

        private async Task UpdateLeaveBalanceForApprovedRequestAsync(LeaveRequest leaveRequest)
        {
            var balance = await _context.EmployeeLeaveBalances
                .FirstOrDefaultAsync(elb => elb.EmployeeId == leaveRequest.EmployeeId && 
                                          elb.LeavePolicyId == leaveRequest.LeavePolicyId && 
                                          elb.Year == leaveRequest.StartDate.Year);

            if (balance != null)
            {
                balance.UsedDays += leaveRequest.RequestedDays;
                balance.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
