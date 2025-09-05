using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.DTOs
{
    // DTOs for Work Schedule Management
    public class WorkShiftDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public ShiftType Type { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }
        public int WorkingHours { get; set; }
        public bool IsNightShift { get; set; }
        public int? FlexibleMinutes { get; set; }
        public bool AllowOvertime { get; set; }
        public int? MaxOvertimeHours { get; set; }
        public int ApplicableDays { get; set; }
        public ShiftStatus Status { get; set; }
    }

    public class CreateWorkShiftDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Code { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public ShiftType Type { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }

        [Range(1, 24)]
        public int WorkingHours { get; set; }

        public bool IsNightShift { get; set; } = false;

        [Range(0, 120)]
        public int? FlexibleMinutes { get; set; }

        public bool AllowOvertime { get; set; } = true;

        [Range(0, 12)]
        public int? MaxOvertimeHours { get; set; }

        public int ApplicableDays { get; set; } = 127; // All days by default
    }

    public class EmployeeShiftAssignmentDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int WorkShiftId { get; set; }
        public string WorkShiftName { get; set; } = string.Empty;
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsDefaultShift { get; set; }
        public int? RotationOrder { get; set; }
        public int? RotationCycleDays { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateEmployeeShiftAssignmentDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int WorkShiftId { get; set; }

        [Required]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public bool IsDefaultShift { get; set; } = false;

        public int? RotationOrder { get; set; }

        public int? RotationCycleDays { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class WorkScheduleDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int WorkShiftId { get; set; }
        public string WorkShiftName { get; set; } = string.Empty;
        public DateTime WorkDate { get; set; }
        public TimeSpan? ActualStartTime { get; set; }
        public TimeSpan? ActualEndTime { get; set; }
        public bool IsPlanned { get; set; }
        public int? ProjectId { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateWorkScheduleDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int WorkShiftId { get; set; }

        [Required]
        public DateTime WorkDate { get; set; }

        public bool IsPlanned { get; set; } = true;

        public int? ProjectId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    // Bulk schedule creation
    public class BulkScheduleCreateDto
    {
        [Required]
        public List<int> EmployeeIds { get; set; } = new List<int>();

        [Required]
        public int WorkShiftId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public List<DayOfWeek> SelectedDays { get; set; } = new List<DayOfWeek>();

        public bool SkipHolidays { get; set; } = true;

        public string? Notes { get; set; }
    }
}
