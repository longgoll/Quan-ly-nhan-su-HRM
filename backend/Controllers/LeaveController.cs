using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Services;
using backend.DTOs;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;

        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        // Leave Policy Management
        [HttpGet("policies")]
        public async Task<ActionResult<IEnumerable<LeavePolicyDto>>> GetLeavePolicies()
        {
            var policies = await _leaveService.GetAllLeavePoliciesAsync();
            return Ok(policies);
        }

        [HttpGet("policies/{id}")]
        public async Task<ActionResult<LeavePolicyDto>> GetLeavePolicy(int id)
        {
            var policy = await _leaveService.GetLeavePolicyByIdAsync(id);
            if (policy == null)
                return NotFound();

            return Ok(policy);
        }

        [HttpPost("policies")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult<LeavePolicyDto>> CreateLeavePolicy([FromBody] CreateLeavePolicyDto createDto)
        {
            try
            {
                var policy = await _leaveService.CreateLeavePolicyAsync(createDto);
                return CreatedAtAction(nameof(GetLeavePolicy), new { id = policy.Id }, policy);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("policies/{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult<LeavePolicyDto>> UpdateLeavePolicy(int id, [FromBody] CreateLeavePolicyDto updateDto)
        {
            try
            {
                var policy = await _leaveService.UpdateLeavePolicyAsync(id, updateDto);
                if (policy == null)
                    return NotFound();

                return Ok(policy);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("policies/{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> DeleteLeavePolicy(int id)
        {
            var result = await _leaveService.DeleteLeavePolicyAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // Employee Leave Balance Management
        [HttpGet("balances")]
        public async Task<ActionResult<IEnumerable<EmployeeLeaveBalanceDto>>> GetLeaveBalances(
            [FromQuery] int? employeeId,
            [FromQuery] int? year)
        {
            // If not admin/hr/manager, only allow viewing own balances
            if (!IsInRole("Admin", "HR", "Manager"))
            {
                employeeId = GetCurrentEmployeeId();
            }

            if (!employeeId.HasValue)
                return BadRequest("Employee ID is required");

            var balances = await _leaveService.GetEmployeeLeaveBalancesAsync(employeeId.Value, year);
            return Ok(balances);
        }

        [HttpGet("balances/{employeeId}/{leavePolicyId}/{year}")]
        public async Task<ActionResult<EmployeeLeaveBalanceDto>> GetLeaveBalance(int employeeId, int leavePolicyId, int year)
        {
            // Check access permissions
            if (!IsInRole("Admin", "HR", "Manager") && employeeId != GetCurrentEmployeeId())
                return Forbid();

            var balance = await _leaveService.GetLeaveBalanceAsync(employeeId, leavePolicyId, year);
            if (balance == null)
                return NotFound();

            return Ok(balance);
        }

        [HttpPost("balances/initialize/{year}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> InitializeLeaveBalancesForYear(int year)
        {
            try
            {
                var result = await _leaveService.InitializeLeaveBalancesForYearAsync(year);
                if (result)
                    return Ok(new { message = $"Leave balances initialized for year {year}" });
                else
                    return BadRequest(new { message = "Failed to initialize leave balances" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("balances/adjust")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> AdjustLeaveBalance([FromBody] LeaveBalanceAdjustmentDto adjustmentDto)
        {
            try
            {
                var result = await _leaveService.AdjustLeaveBalanceAsync(adjustmentDto);
                if (result)
                    return Ok(new { message = "Leave balance adjusted successfully" });
                else
                    return BadRequest(new { message = "Failed to adjust leave balance" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Leave Request Management
        [HttpGet("requests")]
        public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetLeaveRequests([FromQuery] LeaveReportFilterDto filter)
        {
            // If not admin/hr/manager, only allow viewing own requests
            if (!IsInRole("Admin", "HR", "Manager"))
            {
                filter.EmployeeId = GetCurrentEmployeeId();
            }

            var requests = await _leaveService.GetLeaveRequestsAsync(filter);
            return Ok(requests);
        }

        [HttpGet("requests/{id}")]
        public async Task<ActionResult<LeaveRequestDto>> GetLeaveRequest(int id)
        {
            var request = await _leaveService.GetLeaveRequestByIdAsync(id);
            if (request == null)
                return NotFound();

            // Check access permissions
            if (!IsInRole("Admin", "HR", "Manager") && request.EmployeeId != GetCurrentEmployeeId())
                return Forbid();

            return Ok(request);
        }

        [HttpPost("requests")]
        public async Task<ActionResult<LeaveRequestDto>> CreateLeaveRequest([FromBody] CreateLeaveRequestDto createDto)
        {
            try
            {
                var employeeId = GetCurrentEmployeeId();
                var request = await _leaveService.CreateLeaveRequestAsync(employeeId, createDto);
                return CreatedAtAction(nameof(GetLeaveRequest), new { id = request.Id }, request);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("requests/{id}")]
        public async Task<ActionResult<LeaveRequestDto>> UpdateLeaveRequest(int id, [FromBody] CreateLeaveRequestDto updateDto)
        {
            try
            {
                // Check if the request belongs to the current user (unless admin/hr/manager)
                var existingRequest = await _leaveService.GetLeaveRequestByIdAsync(id);
                if (existingRequest == null)
                    return NotFound();

                if (!IsInRole("Admin", "HR", "Manager") && existingRequest.EmployeeId != GetCurrentEmployeeId())
                    return Forbid();

                var request = await _leaveService.UpdateLeaveRequestAsync(id, updateDto);
                if (request == null)
                    return NotFound();

                return Ok(request);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("requests/{id}/cancel")]
        public async Task<IActionResult> CancelLeaveRequest(int id)
        {
            try
            {
                var employeeId = GetCurrentEmployeeId();
                var result = await _leaveService.CancelLeaveRequestAsync(id, employeeId);
                if (!result)
                    return NotFound();

                return Ok(new { message = "Leave request cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("requests/{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> DeleteLeaveRequest(int id)
        {
            try
            {
                var result = await _leaveService.DeleteLeaveRequestAsync(id);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Leave Approval Workflow
        [HttpPost("approvals")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<LeaveRequestDto>> ProcessLeaveApproval([FromBody] ProcessLeaveApprovalDto approvalDto)
        {
            try
            {
                var approverId = GetCurrentEmployeeId();
                var request = await _leaveService.ProcessLeaveApprovalAsync(approverId, approvalDto);
                return Ok(request);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("approvals/pending")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetPendingApprovals()
        {
            var approverId = GetCurrentEmployeeId();
            var pendingRequests = await _leaveService.GetPendingApprovalsAsync(approverId);
            return Ok(pendingRequests);
        }

        [HttpPost("requests/{id}/setup-workflow")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> SetupApprovalWorkflow(int id, [FromBody] List<int> approverIds)
        {
            try
            {
                var result = await _leaveService.SetupApprovalWorkflowAsync(id, approverIds);
                if (result)
                    return Ok(new { message = "Approval workflow setup successfully" });
                else
                    return BadRequest(new { message = "Failed to setup approval workflow" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Public Holiday Management
        [HttpGet("holidays")]
        public async Task<ActionResult<IEnumerable<PublicHolidayDto>>> GetPublicHolidays(
            [FromQuery] int? year,
            [FromQuery] int? departmentId)
        {
            var holidays = await _leaveService.GetPublicHolidaysAsync(year, departmentId);
            return Ok(holidays);
        }

        [HttpGet("holidays/{id}")]
        public async Task<ActionResult<PublicHolidayDto>> GetPublicHoliday(int id)
        {
            var holiday = await _leaveService.GetPublicHolidayByIdAsync(id);
            if (holiday == null)
                return NotFound();

            return Ok(holiday);
        }

        [HttpPost("holidays")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult<PublicHolidayDto>> CreatePublicHoliday([FromBody] CreatePublicHolidayDto createDto)
        {
            try
            {
                var holiday = await _leaveService.CreatePublicHolidayAsync(createDto);
                return CreatedAtAction(nameof(GetPublicHoliday), new { id = holiday.Id }, holiday);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("holidays/{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult<PublicHolidayDto>> UpdatePublicHoliday(int id, [FromBody] CreatePublicHolidayDto updateDto)
        {
            try
            {
                var holiday = await _leaveService.UpdatePublicHolidayAsync(id, updateDto);
                if (holiday == null)
                    return NotFound();

                return Ok(holiday);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("holidays/{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> DeletePublicHoliday(int id)
        {
            var result = await _leaveService.DeletePublicHolidayAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // Reports and Analytics
        [HttpGet("calendar")]
        public async Task<ActionResult<IEnumerable<LeaveCalendarDto>>> GetLeaveCalendar(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? departmentId)
        {
            var calendar = await _leaveService.GetLeaveCalendarAsync(startDate, endDate, departmentId);
            return Ok(calendar);
        }

        [HttpGet("history/{employeeId}/{year}")]
        public async Task<ActionResult<EmployeeLeaveHistoryDto>> GetEmployeeLeaveHistory(int employeeId, int year)
        {
            // Check access permissions
            if (!IsInRole("Admin", "HR", "Manager") && employeeId != GetCurrentEmployeeId())
                return Forbid();

            try
            {
                var history = await _leaveService.GetEmployeeLeaveHistoryAsync(employeeId, year);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("department-balances/{departmentId}/{year}")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<IEnumerable<EmployeeLeaveBalanceDto>>> GetDepartmentLeaveBalances(int departmentId, int year)
        {
            var balances = await _leaveService.GetDepartmentLeaveBalancesAsync(departmentId, year);
            return Ok(balances);
        }

        // Helper Endpoints
        [HttpGet("can-request")]
        public async Task<ActionResult<bool>> CanRequestLeave(
            [FromQuery] int? employeeId,
            [FromQuery] int leavePolicyId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var currentEmployeeId = employeeId ?? GetCurrentEmployeeId();
            
            // Check access permissions
            if (!IsInRole("Admin", "HR", "Manager") && currentEmployeeId != GetCurrentEmployeeId())
                return Forbid();

            var canRequest = await _leaveService.CanRequestLeaveAsync(currentEmployeeId, leavePolicyId, startDate, endDate);
            return Ok(canRequest);
        }

        [HttpGet("calculate-days")]
        public async Task<ActionResult<decimal>> CalculateRequestedDays(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] bool includeWeekends = false)
        {
            var days = await _leaveService.CalculateRequestedDaysAsync(startDate, endDate, includeWeekends);
            return Ok(days);
        }

        [HttpGet("check-conflict")]
        public async Task<ActionResult<bool>> CheckLeaveConflict(
            [FromQuery] int? employeeId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? excludeRequestId)
        {
            var currentEmployeeId = employeeId ?? GetCurrentEmployeeId();
            
            // Check access permissions
            if (!IsInRole("Admin", "HR", "Manager") && currentEmployeeId != GetCurrentEmployeeId())
                return Forbid();

            var hasConflict = await _leaveService.HasLeaveConflictAsync(currentEmployeeId, startDate, endDate, excludeRequestId);
            return Ok(hasConflict);
        }

        [HttpGet("applicable-policies")]
        public async Task<ActionResult<IEnumerable<LeavePolicyDto>>> GetApplicableLeavePolicies([FromQuery] int? employeeId)
        {
            var currentEmployeeId = employeeId ?? GetCurrentEmployeeId();
            
            // Check access permissions
            if (!IsInRole("Admin", "HR", "Manager") && currentEmployeeId != GetCurrentEmployeeId())
                return Forbid();

            var policies = await _leaveService.GetApplicableLeavePoliciesAsync(currentEmployeeId);
            return Ok(policies);
        }

        // Helper Methods
        private int GetCurrentEmployeeId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User ID not found in token");

            // You might need to implement a method to get Employee ID from User ID
            // For now, assuming the claim contains employee ID directly
            if (int.TryParse(userIdClaim.Value, out int employeeId))
                return employeeId;

            throw new UnauthorizedAccessException("Invalid employee ID in token");
        }

        private bool IsInRole(params string[] roles)
        {
            return roles.Any(role => User.IsInRole(role));
        }
    }
}
