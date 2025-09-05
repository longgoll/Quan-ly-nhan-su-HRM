using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.DTOs
{
    // DTOs for Attendance Management
    public class AttendanceDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; }
        public int WorkShiftId { get; set; }
        public string WorkShiftName { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public DateTime? BreakStartTime { get; set; }
        public DateTime? BreakEndTime { get; set; }
        public string? CheckInLocation { get; set; }
        public string? CheckOutLocation { get; set; }
        public int? TotalWorkingMinutes { get; set; }
        public int? BreakMinutes { get; set; }
        public int? LateMinutes { get; set; }
        public int? EarlyLeaveMinutes { get; set; }
        public int? OvertimeMinutes { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Notes { get; set; }
        public string? ManagerNotes { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class CheckInDto
    {
        [Required]
        public DateTime CheckInTime { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [StringLength(500)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? DeviceId { get; set; }

        [StringLength(100)]
        public string? DeviceType { get; set; }
    }

    public class CheckOutDto
    {
        [Required]
        public DateTime CheckOutTime { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [StringLength(500)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? DeviceId { get; set; }

        [StringLength(100)]
        public string? DeviceType { get; set; }
    }

    public class BreakTimeDto
    {
        [Required]
        public AttendanceType Type { get; set; } // BreakStart or BreakEnd

        [Required]
        public DateTime Timestamp { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [StringLength(500)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class AttendanceDetailDto
    {
        public int Id { get; set; }
        public AttendanceType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Location { get; set; }
        public string? DeviceId { get; set; }
        public string? DeviceType { get; set; }
        public string? IpAddress { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Notes { get; set; }
    }

    public class AttendanceSummaryDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalWorkingDays { get; set; }
        public int ActualWorkingDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public int EarlyLeaveDays { get; set; }
        public int TotalWorkingMinutes { get; set; }
        public int StandardWorkingMinutes { get; set; }
        public int OvertimeMinutes { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }
        public int VacationDays { get; set; }
        public int SickLeaveDays { get; set; }
        public int PersonalLeaveDays { get; set; }

        // Calculated properties
        public double AttendanceRate => TotalWorkingDays > 0 ? (double)ActualWorkingDays / TotalWorkingDays * 100 : 0;
        public double OvertimeHours => OvertimeMinutes / 60.0;
        public double TotalWorkingHours => TotalWorkingMinutes / 60.0;
    }

    public class AttendanceApprovalDto
    {
        [Required]
        public int AttendanceId { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        [StringLength(1000)]
        public string? ManagerNotes { get; set; }
    }

    public class AttendanceReportFilterDto
    {
        public int? EmployeeId { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public AttendanceStatus? Status { get; set; }
        public bool IncludeDetails { get; set; } = false;
    }

    public class DailyAttendanceReportDto
    {
        public DateTime Date { get; set; }
        public int TotalEmployees { get; set; }
        public int PresentEmployees { get; set; }
        public int AbsentEmployees { get; set; }
        public int LateEmployees { get; set; }
        public int EarlyLeaveEmployees { get; set; }
        public List<AttendanceDto> AttendanceRecords { get; set; } = new List<AttendanceDto>();
    }

    public class EmployeeAttendanceHistoryDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<AttendanceDto> AttendanceRecords { get; set; } = new List<AttendanceDto>();
        public AttendanceSummaryDto Summary { get; set; } = new AttendanceSummaryDto();
    }
}
