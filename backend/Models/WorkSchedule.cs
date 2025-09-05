using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    // Enum cho loại ca làm việc
    public enum ShiftType
    {
        Fixed = 0,      // Ca cố định
        Rotating = 1,   // Xoay ca
        Project = 2,    // Theo dự án
        Flexible = 3,   // Ca linh hoạt
        PartTime = 4    // Bán thời gian
    }

    // Enum cho trạng thái ca làm việc
    public enum ShiftStatus
    {
        Active = 0,
        Inactive = 1,
        Temporary = 2
    }

    // Model định nghĩa ca làm việc
    public class WorkShift
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // Tên ca làm việc

        [StringLength(20)]
        public string? Code { get; set; } // Mã ca làm việc

        [StringLength(500)]
        public string? Description { get; set; }

        public ShiftType Type { get; set; }

        public TimeSpan StartTime { get; set; } // Giờ bắt đầu
        public TimeSpan EndTime { get; set; }   // Giờ kết thúc

        public TimeSpan? BreakStartTime { get; set; } // Giờ bắt đầu nghỉ trưa
        public TimeSpan? BreakEndTime { get; set; }   // Giờ kết thúc nghỉ trưa

        public int WorkingHours { get; set; } // Số giờ làm việc trong ca

        public bool IsNightShift { get; set; } // Ca đêm (qua ngày)

        // Thiết lập thời gian linh hoạt
        public int? FlexibleMinutes { get; set; } // Phút linh hoạt cho check-in/out

        // Thiết lập overtime
        public bool AllowOvertime { get; set; } = true;
        public int? MaxOvertimeHours { get; set; } // Giờ làm thêm tối đa

        // Các ngày trong tuần áp dụng (bit mask: 1=CN, 2=T2, 4=T3, ...)
        public int ApplicableDays { get; set; } = 127; // Mặc định tất cả các ngày

        public ShiftStatus Status { get; set; } = ShiftStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<EmployeeShiftAssignment> EmployeeShiftAssignments { get; set; } = new List<EmployeeShiftAssignment>();
        public virtual ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
    }

    // Model phân ca cho nhân viên
    public class EmployeeShiftAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        public int WorkShiftId { get; set; }
        [ForeignKey("WorkShiftId")]
        public WorkShift WorkShift { get; set; } = null!;

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        public bool IsDefaultShift { get; set; } = false; // Ca mặc định

        // Cho ca xoay ca
        public int? RotationOrder { get; set; } // Thứ tự trong chu kỳ xoay ca
        public int? RotationCycleDays { get; set; } // Số ngày trong một chu kỳ

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    // Model lịch làm việc chi tiết
    public class WorkSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        public int WorkShiftId { get; set; }
        [ForeignKey("WorkShiftId")]
        public WorkShift WorkShift { get; set; } = null!;

        [Required]
        public DateTime WorkDate { get; set; } // Ngày làm việc

        public TimeSpan? ActualStartTime { get; set; } // Giờ bắt đầu thực tế
        public TimeSpan? ActualEndTime { get; set; }   // Giờ kết thúc thực tế

        public bool IsPlanned { get; set; } = true; // Có phải là lịch đã lập trước

        // Cho dự án
        public int? ProjectId { get; set; } // Tham chiếu đến dự án (nếu có)
        
        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
