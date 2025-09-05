using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    // Enum cho loại chấm công
    public enum AttendanceType
    {
        CheckIn = 0,
        CheckOut = 1,
        BreakStart = 2,
        BreakEnd = 3
    }

    // Enum cho trạng thái chấm công
    public enum AttendanceStatus
    {
        OnTime = 0,     // Đúng giờ
        Late = 1,       // Đi trễ
        Early = 2,      // Về sớm
        Overtime = 3,   // Làm thêm giờ
        NoShow = 4,     // Không có mặt
        Approved = 5    // Đã được phê duyệt
    }

    // Model chấm công
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        public DateTime AttendanceDate { get; set; }

        [Required]
        public int WorkShiftId { get; set; }
        [ForeignKey("WorkShiftId")]
        public WorkShift WorkShift { get; set; } = null!;

        // Thời gian chấm công
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public DateTime? BreakStartTime { get; set; }
        public DateTime? BreakEndTime { get; set; }

        // Vị trí chấm công (GPS)
        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }
        public double? CheckOutLatitude { get; set; }
        public double? CheckOutLongitude { get; set; }

        [StringLength(500)]
        public string? CheckInLocation { get; set; }
        [StringLength(500)]
        public string? CheckOutLocation { get; set; }

        // Tính toán thời gian
        public int? TotalWorkingMinutes { get; set; }  // Tổng phút làm việc
        public int? BreakMinutes { get; set; }         // Phút nghỉ trưa
        public int? LateMinutes { get; set; }          // Phút đi trễ
        public int? EarlyLeaveMinutes { get; set; }    // Phút về sớm
        public int? OvertimeMinutes { get; set; }      // Phút làm thêm

        public AttendanceStatus Status { get; set; }

        // Ghi chú và phê duyệt
        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(1000)]
        public string? ManagerNotes { get; set; }

        public int? ApprovedById { get; set; }
        [ForeignKey("ApprovedById")]
        public Employee? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        // Hình ảnh chấm công (nếu có)
        [StringLength(500)]
        public string? CheckInPhotoUrl { get; set; }
        [StringLength(500)]
        public string? CheckOutPhotoUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    // Model chi tiết chấm công
    public class AttendanceDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AttendanceId { get; set; }
        [ForeignKey("AttendanceId")]
        public Attendance Attendance { get; set; } = null!;

        [Required]
        public AttendanceType Type { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        // Vị trí
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [StringLength(500)]
        public string? Location { get; set; }

        // Thiết bị chấm công
        [StringLength(100)]
        public string? DeviceId { get; set; }

        [StringLength(100)]
        public string? DeviceType { get; set; } // Mobile, Web, Biometric, etc.

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Model báo cáo chấm công tổng hợp
    public class AttendanceSummary
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        public int Year { get; set; }

        [Required]
        public int Month { get; set; }

        // Thống kê ngày làm việc
        public int TotalWorkingDays { get; set; }
        public int ActualWorkingDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public int EarlyLeaveDays { get; set; }

        // Thống kê thời gian (tính bằng phút)
        public int TotalWorkingMinutes { get; set; }
        public int StandardWorkingMinutes { get; set; }
        public int OvertimeMinutes { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }

        // Nghỉ phép
        public int VacationDays { get; set; }
        public int SickLeaveDays { get; set; }
        public int PersonalLeaveDays { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
