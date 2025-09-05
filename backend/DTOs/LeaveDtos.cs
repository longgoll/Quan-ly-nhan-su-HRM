using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.DTOs
{
    // DTOs for Leave Management
    public class LeavePolicyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public LeaveType LeaveType { get; set; }
        public int AnnualAllowanceDays { get; set; }
        public int MaxCarryForwardDays { get; set; }
        public int MaxConsecutiveDays { get; set; }
        public int MinAdvanceNoticeDays { get; set; }
        public bool RequiresDocumentation { get; set; }
        public bool IsPaid { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int? PositionId { get; set; }
        public string? PositionName { get; set; }
        public int MinTenureMonths { get; set; }
        public bool IsActive { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    public class CreateLeavePolicyDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; }

        [Range(0, 365)]
        public int AnnualAllowanceDays { get; set; }

        [Range(0, 30)]
        public int MaxCarryForwardDays { get; set; } = 0;

        [Range(1, 365)]
        public int MaxConsecutiveDays { get; set; } = 365;

        [Range(0, 30)]
        public int MinAdvanceNoticeDays { get; set; } = 1;

        public bool RequiresDocumentation { get; set; } = false;

        public bool IsPaid { get; set; } = true;

        public int? DepartmentId { get; set; }

        public int? PositionId { get; set; }

        [Range(0, 360)]
        public int MinTenureMonths { get; set; } = 0;

        [Required]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }
    }

    public class EmployeeLeaveBalanceDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int LeavePolicyId { get; set; }
        public string LeavePolicyName { get; set; } = string.Empty;
        public LeaveType LeaveType { get; set; }
        public int Year { get; set; }
        public decimal AllocatedDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal CarriedForwardDays { get; set; }
        public decimal AdjustmentDays { get; set; }
        public decimal RemainingDays { get; set; }
    }

    public class LeaveRequestDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int LeavePolicyId { get; set; }
        public string LeavePolicyName { get; set; } = string.Empty;
        public LeaveType LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal RequestedDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFileName { get; set; }
        public int? CoverEmployeeId { get; set; }
        public string? CoverEmployeeName { get; set; }
        public string? CoverNotes { get; set; }
        public LeaveStatus Status { get; set; }
        public string? ManagerComments { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<LeaveApprovalWorkflowDto> ApprovalWorkflow { get; set; } = new List<LeaveApprovalWorkflowDto>();
    }

    public class CreateLeaveRequestDto
    {
        [Required]
        public int LeavePolicyId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? AttachmentUrl { get; set; }

        [StringLength(100)]
        public string? AttachmentFileName { get; set; }

        public int? CoverEmployeeId { get; set; }

        [StringLength(1000)]
        public string? CoverNotes { get; set; }

        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(500)]
        public string? EmergencyContactAddress { get; set; }
    }

    public class LeaveApprovalWorkflowDto
    {
        public int Id { get; set; }
        public int LeaveRequestId { get; set; }
        public int ApproverEmployeeId { get; set; }
        public string ApproverEmployeeName { get; set; } = string.Empty;
        public int Order { get; set; }
        public LeaveStatus Status { get; set; }
        public string? Comments { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    public class ProcessLeaveApprovalDto
    {
        [Required]
        public int LeaveRequestId { get; set; }

        [Required]
        public LeaveStatus Status { get; set; }

        [StringLength(1000)]
        public string? Comments { get; set; }
    }

    public class PublicHolidayDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public bool IsPaid { get; set; }
        public bool IsMandatory { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreatePublicHolidayDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public bool IsPaid { get; set; } = true;

        public bool IsMandatory { get; set; } = true;

        public int? DepartmentId { get; set; }
    }

    public class LeaveBalanceAdjustmentDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int LeavePolicyId { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public decimal AdjustmentDays { get; set; }

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class LeaveReportFilterDto
    {
        public int? EmployeeId { get; set; }
        public int? DepartmentId { get; set; }
        public LeaveType? LeaveType { get; set; }
        public LeaveStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
    }

    public class LeaveCalendarDto
    {
        public DateTime Date { get; set; }
        public List<LeaveCalendarEntryDto> Entries { get; set; } = new List<LeaveCalendarEntryDto>();
        public bool IsPublicHoliday { get; set; }
        public string? PublicHolidayName { get; set; }
    }

    public class LeaveCalendarEntryDto
    {
        public int LeaveRequestId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public LeaveType LeaveType { get; set; }
        public LeaveStatus Status { get; set; }
        public bool IsStartDate { get; set; }
        public bool IsEndDate { get; set; }
        public decimal RequestedDays { get; set; }
    }

    public class EmployeeLeaveHistoryDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int Year { get; set; }
        public List<EmployeeLeaveBalanceDto> LeaveBalances { get; set; } = new List<EmployeeLeaveBalanceDto>();
        public List<LeaveRequestDto> LeaveRequests { get; set; } = new List<LeaveRequestDto>();
        public decimal TotalLeaveDaysTaken { get; set; }
        public decimal TotalLeaveBalance { get; set; }
    }
}
