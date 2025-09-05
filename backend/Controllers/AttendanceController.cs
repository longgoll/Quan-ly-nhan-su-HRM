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
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // Check-in/Check-out Operations
        [HttpPost("check-in")]
        public async Task<ActionResult<AttendanceDto>> CheckIn([FromBody] CheckInDto checkInDto)
        {
            try
            {
                var employeeId = GetCurrentEmployeeId();
                var attendance = await _attendanceService.CheckInAsync(employeeId, checkInDto);
                return Ok(attendance);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("check-out")]
        public async Task<ActionResult<AttendanceDto>> CheckOut([FromBody] CheckOutDto checkOutDto)
        {
            try
            {
                var employeeId = GetCurrentEmployeeId();
                var attendance = await _attendanceService.CheckOutAsync(employeeId, checkOutDto);
                return Ok(attendance);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("break-time")]
        public async Task<ActionResult<AttendanceDto>> RecordBreakTime([FromBody] BreakTimeDto breakTimeDto)
        {
            try
            {
                var employeeId = GetCurrentEmployeeId();
                var attendance = await _attendanceService.RecordBreakTimeAsync(employeeId, breakTimeDto);
                return Ok(attendance);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Attendance Management
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendances([FromQuery] AttendanceReportFilterDto filter)
        {
            // If not admin/hr/manager, only allow viewing own records
            if (!IsInRole("Admin", "HR", "Manager"))
            {
                filter.EmployeeId = GetCurrentEmployeeId();
            }

            var attendances = await _attendanceService.GetAttendancesAsync(filter);
            return Ok(attendances);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AttendanceDto>> GetAttendance(int id)
        {
            var attendance = await _attendanceService.GetAttendanceByIdAsync(id);
            if (attendance == null)
                return NotFound();

            // Check access permissions
            if (!IsInRole("Admin", "HR", "Manager") && attendance.EmployeeId != GetCurrentEmployeeId())
                return Forbid();

            return Ok(attendance);
        }

        [HttpGet("today")]
        public async Task<ActionResult<AttendanceDto>> GetTodayAttendance()
        {
            var employeeId = GetCurrentEmployeeId();
            var attendance = await _attendanceService.GetTodayAttendanceAsync(employeeId);
            
            if (attendance == null)
                return NotFound(new { message = "No attendance record found for today" });

            return Ok(attendance);
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<AttendanceDto>> ApproveAttendance(int id, [FromBody] AttendanceApprovalDto approvalDto)
        {
            try
            {
                approvalDto.AttendanceId = id;
                var attendance = await _attendanceService.UpdateAttendanceAsync(id, approvalDto);
                if (attendance == null)
                    return NotFound();

                return Ok(attendance);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var result = await _attendanceService.DeleteAttendanceAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // Attendance Details
        [HttpGet("{id}/details")]
        public async Task<ActionResult<IEnumerable<AttendanceDetailDto>>> GetAttendanceDetails(int id)
        {
            var attendance = await _attendanceService.GetAttendanceByIdAsync(id);
            if (attendance == null)
                return NotFound();

            // Check access permissions
            if (!IsInRole("Admin", "HR", "Manager") && attendance.EmployeeId != GetCurrentEmployeeId())
                return Forbid();

            var details = await _attendanceService.GetAttendanceDetailsAsync(id);
            return Ok(details);
        }

        // Reports and Analytics
        [HttpGet("summary/monthly")]
        public async Task<ActionResult<AttendanceSummaryDto>> GetMonthlySummary(
            [FromQuery] int? employeeId,
            [FromQuery] int year,
            [FromQuery] int month)
        {
            // If not admin/hr/manager, only allow viewing own summary
            if (!IsInRole("Admin", "HR", "Manager"))
            {
                employeeId = GetCurrentEmployeeId();
            }

            if (!employeeId.HasValue)
                return BadRequest("Employee ID is required");

            var summary = await _attendanceService.GetMonthlySummaryAsync(employeeId.Value, year, month);
            if (summary == null)
                return NotFound();

            return Ok(summary);
        }

        [HttpGet("summary/department/{departmentId}")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<IEnumerable<AttendanceSummaryDto>>> GetDepartmentSummary(
            int departmentId,
            [FromQuery] int year,
            [FromQuery] int month)
        {
            var summaries = await _attendanceService.GetDepartmentSummaryAsync(departmentId, year, month);
            return Ok(summaries);
        }

        [HttpGet("reports/daily")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<DailyAttendanceReportDto>> GetDailyReport(
            [FromQuery] DateTime date,
            [FromQuery] int? departmentId)
        {
            var report = await _attendanceService.GetDailyReportAsync(date, departmentId);
            return Ok(report);
        }

        [HttpGet("reports/employee-history/{employeeId}")]
        public async Task<ActionResult<EmployeeAttendanceHistoryDto>> GetEmployeeAttendanceHistory(
            int employeeId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            // Check access permissions
            if (!IsInRole("Admin", "HR", "Manager") && employeeId != GetCurrentEmployeeId())
                return Forbid();

            try
            {
                var history = await _attendanceService.GetEmployeeAttendanceHistoryAsync(employeeId, startDate, endDate);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Bulk Operations
        [HttpPost("generate-monthly-summary")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GenerateMonthlyAttendanceSummary(
            [FromQuery] int year,
            [FromQuery] int month)
        {
            try
            {
                var result = await _attendanceService.GenerateMonthlyAttendanceSummaryAsync(year, month);
                if (result)
                    return Ok(new { message = "Monthly attendance summary generated successfully" });
                else
                    return BadRequest(new { message = "Failed to generate monthly attendance summary" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("approve-multiple")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> ApproveMultipleAttendances([FromBody] ApproveMultipleRequest request)
        {
            try
            {
                var approverId = GetCurrentEmployeeId();
                var result = await _attendanceService.ApproveMultipleAttendancesAsync(request.AttendanceIds, approverId, request.Notes);
                
                if (result)
                    return Ok(new { message = "Attendances approved successfully" });
                else
                    return BadRequest(new { message = "Failed to approve attendances" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Status Check Endpoints
        [HttpGet("status/checked-in")]
        public async Task<ActionResult<bool>> HasCheckedInToday()
        {
            var employeeId = GetCurrentEmployeeId();
            var hasCheckedIn = await _attendanceService.HasCheckedInTodayAsync(employeeId);
            return Ok(hasCheckedIn);
        }

        [HttpGet("status/checked-out")]
        public async Task<ActionResult<bool>> HasCheckedOutToday()
        {
            var employeeId = GetCurrentEmployeeId();
            var hasCheckedOut = await _attendanceService.HasCheckedOutTodayAsync(employeeId);
            return Ok(hasCheckedOut);
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

    // Supporting DTOs
    public class ApproveMultipleRequest
    {
        public List<int> AttendanceIds { get; set; } = new List<int>();
        public string? Notes { get; set; }
    }
}
