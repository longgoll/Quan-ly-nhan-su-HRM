using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    // Enum cho loại nghỉ phép
    public enum LeaveType
    {
        Annual = 0,         // Phép năm
        Sick = 1,           // Nghỉ ốm
        Personal = 2,       // Nghỉ việc riêng
        Maternity = 3,      // Nghỉ thai sản
        Paternity = 4,      // Nghỉ chăm con
        Bereavement = 5,    // Nghỉ tang lễ
        Study = 6,          // Nghỉ học tập
        Business = 7,       // Công tác
        Compensatory = 8,   // Nghỉ bù
        Unpaid = 9,         // Nghỉ không lương
        Marriage = 10,      // Nghỉ cưới
        Emergency = 11      // Nghỉ khẩn cấp
    }

    // Enum cho trạng thái đơn nghỉ phép
    public enum LeaveStatus
    {
        Pending = 0,    // Chờ phê duyệt
        Approved = 1,   // Đã phê duyệt
        Rejected = 2,   // Bị từ chối
        Cancelled = 3,  // Đã hủy
        InProgress = 4, // Đang diễn ra
        Completed = 5   // Đã hoàn thành
    }

    // Model chính sách nghỉ phép
    public class LeavePolicy
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public LeaveType LeaveType { get; set; }

        // Số ngày phép được cấp
        public int AnnualAllowanceDays { get; set; }

        // Số ngày tối đa có thể tích lũy
        public int MaxCarryForwardDays { get; set; } = 0;

        // Số ngày tối đa có thể nghỉ liên tiếp
        public int MaxConsecutiveDays { get; set; } = 365;

        // Số ngày tối thiểu phải báo trước
        public int MinAdvanceNoticeDays { get; set; } = 1;

        // Có cần chứng từ không (VD: giấy bác sĩ cho nghỉ ốm)
        public bool RequiresDocumentation { get; set; } = false;

        // Có tính lương không
        public bool IsPaid { get; set; } = true;

        // Áp dụng cho phòng ban nào (null = tất cả)
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        // Áp dụng cho vị trí nào (null = tất cả)
        public int? PositionId { get; set; }
        [ForeignKey("PositionId")]
        public Position? Position { get; set; }

        // Thâm niên tối thiểu (tháng)
        public int MinTenureMonths { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<EmployeeLeaveBalance> EmployeeLeaveBalances { get; set; } = new List<EmployeeLeaveBalance>();
        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    }

    // Model số dư nghỉ phép của nhân viên
    public class EmployeeLeaveBalance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        public int LeavePolicyId { get; set; }
        [ForeignKey("LeavePolicyId")]
        public LeavePolicy LeavePolicy { get; set; } = null!;

        [Required]
        public int Year { get; set; }

        // Số ngày được cấp trong năm
        public decimal AllocatedDays { get; set; }

        // Số ngày đã sử dụng
        public decimal UsedDays { get; set; } = 0;

        // Số ngày chuyển từ năm trước
        public decimal CarriedForwardDays { get; set; } = 0;

        // Số ngày điều chỉnh (thưởng/phạt)
        public decimal AdjustmentDays { get; set; } = 0;

        // Số ngày còn lại
        public decimal RemainingDays => AllocatedDays + CarriedForwardDays + AdjustmentDays - UsedDays;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    // Model đơn xin nghỉ phép
    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        public int LeavePolicyId { get; set; }
        [ForeignKey("LeavePolicyId")]
        public LeavePolicy LeavePolicy { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Số ngày nghỉ (có thể là 0.5 cho nghỉ nửa ngày)
        public decimal RequestedDays { get; set; }

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        // File đính kèm (giấy bác sĩ, v.v.)
        [StringLength(500)]
        public string? AttachmentUrl { get; set; }

        [StringLength(100)]
        public string? AttachmentFileName { get; set; }

        // Người thay thế
        public int? CoverEmployeeId { get; set; }
        [ForeignKey("CoverEmployeeId")]
        public Employee? CoverEmployee { get; set; }

        [StringLength(1000)]
        public string? CoverNotes { get; set; }

        // Trạng thái và phê duyệt
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        [StringLength(1000)]
        public string? ManagerComments { get; set; }

        public int? ApprovedById { get; set; }
        [ForeignKey("ApprovedById")]
        public Employee? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        // Liên hệ khẩn cấp
        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(500)]
        public string? EmergencyContactAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<LeaveApprovalWorkflow> LeaveApprovalWorkflows { get; set; } = new List<LeaveApprovalWorkflow>();
    }

    // Model workflow phê duyệt nghỉ phép
    public class LeaveApprovalWorkflow
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LeaveRequestId { get; set; }
        [ForeignKey("LeaveRequestId")]
        public LeaveRequest LeaveRequest { get; set; } = null!;

        [Required]
        public int ApproverEmployeeId { get; set; }
        [ForeignKey("ApproverEmployeeId")]
        public Employee ApproverEmployee { get; set; } = null!;

        public int Order { get; set; } // Thứ tự phê duyệt

        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        [StringLength(1000)]
        public string? Comments { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Model lịch nghỉ chung (ngày lễ, nghỉ tập thể)
    public class PublicHoliday
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        // Có phải ngày nghỉ có lương không
        public bool IsPaid { get; set; } = true;

        // Có phải ngày nghỉ bắt buộc không
        public bool IsMandatory { get; set; } = true;

        // Áp dụng cho phòng ban nào (null = tất cả)
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
