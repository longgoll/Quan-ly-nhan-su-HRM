using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.DTOs
{
    // Employee DTOs
    public class CreateEmployeeDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [StringLength(20)]
        public string? IdentityNumber { get; set; }

        public DateTime? IdentityIssueDate { get; set; }

        [StringLength(100)]
        public string? IdentityIssuePlace { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Province { get; set; }

        [StringLength(10)]
        public string? PostalCode { get; set; }

        [StringLength(20)]
        public string? PersonalPhoneNumber { get; set; }

        [StringLength(255)]
        [EmailAddress]
        public string? PersonalEmail { get; set; }

        public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Single;

        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [StringLength(100)]
        public string? EmergencyContactRelation { get; set; }

        // Work Info
        public int? DepartmentId { get; set; }
        public int? PositionId { get; set; }
        public int? DirectManagerId { get; set; }

        [StringLength(20)]
        public string? EmployeeCode { get; set; }

        [Required]
        public DateTime HireDate { get; set; }

        public decimal? BaseSalary { get; set; }
    }

    public class UpdateEmployeeDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [StringLength(20)]
        public string? IdentityNumber { get; set; }

        public DateTime? IdentityIssueDate { get; set; }

        [StringLength(100)]
        public string? IdentityIssuePlace { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Province { get; set; }

        [StringLength(10)]
        public string? PostalCode { get; set; }

        [StringLength(20)]
        public string? PersonalPhoneNumber { get; set; }

        [StringLength(255)]
        [EmailAddress]
        public string? PersonalEmail { get; set; }

        public MaritalStatus MaritalStatus { get; set; }

        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [StringLength(100)]
        public string? EmergencyContactRelation { get; set; }

        // Work Info
        public int? DepartmentId { get; set; }
        public int? PositionId { get; set; }
        public int? DirectManagerId { get; set; }
        public decimal? BaseSalary { get; set; }
        public EmployeeStatus Status { get; set; }
    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string? IdentityNumber { get; set; }
        public DateTime? IdentityIssueDate { get; set; }
        public string? IdentityIssuePlace { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        public string? PersonalPhoneNumber { get; set; }
        public string? PersonalEmail { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelation { get; set; }
        public string? EmployeeCode { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public EmployeeStatus Status { get; set; }
        public decimal? BaseSalary { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Related entities
        public DepartmentDto? Department { get; set; }
        public PositionDto? Position { get; set; }
        public EmployeeBasicDto? DirectManager { get; set; }
    }

    public class EmployeeBasicDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? DepartmentName { get; set; }
        public string? PositionTitle { get; set; }
    }

    // Department DTOs
    public class CreateDepartmentDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Code { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int? ParentDepartmentId { get; set; }
        public int? ManagerId { get; set; }
    }

    public class UpdateDepartmentDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Code { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int? ParentDepartmentId { get; set; }
        public int? ManagerId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? ParentDepartmentId { get; set; }
        public string? ParentDepartmentName { get; set; }
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int EmployeeCount { get; set; }
        public List<DepartmentDto> SubDepartments { get; set; } = new List<DepartmentDto>();
    }

    // Position DTOs
    public class CreatePositionDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Code { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Requirements { get; set; }

        public int? DepartmentId { get; set; }

        [Range(1, 10)]
        public int Level { get; set; } = 1;

        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
    }

    public class UpdatePositionDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Code { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Requirements { get; set; }

        public int? DepartmentId { get; set; }

        [Range(1, 10)]
        public int Level { get; set; }

        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PositionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int Level { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int EmployeeCount { get; set; }
    }

    // Contract DTOs
    public class CreateContractDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        public ContractType Type { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public decimal Salary { get; set; }

        [StringLength(1000)]
        public string? Terms { get; set; }

        public DateTime? SignedDate { get; set; }
    }

    public class UpdateContractDto
    {
        [Required]
        [StringLength(50)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        public ContractType Type { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public decimal Salary { get; set; }

        [StringLength(1000)]
        public string? Terms { get; set; }

        public ContractStatus Status { get; set; }
        public DateTime? SignedDate { get; set; }
        public string? TerminationReason { get; set; }
        public DateTime? TerminationDate { get; set; }
    }

    public class ContractDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string ContractNumber { get; set; } = string.Empty;
        public ContractType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Salary { get; set; }
        public string? Terms { get; set; }
        public ContractStatus Status { get; set; }
        public DateTime? SignedDate { get; set; }
        public string? TerminationReason { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string? DocumentPath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Work History DTOs
    public class CreateWorkHistoryDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public WorkHistoryType Type { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        public int? OldDepartmentId { get; set; }
        public int? NewDepartmentId { get; set; }
        public int? OldPositionId { get; set; }
        public int? NewPositionId { get; set; }
        public decimal? OldSalary { get; set; }
        public decimal? NewSalary { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class WorkHistoryDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public WorkHistoryType Type { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string? OldDepartmentName { get; set; }
        public string? NewDepartmentName { get; set; }
        public string? OldPositionTitle { get; set; }
        public string? NewPositionTitle { get; set; }
        public decimal? OldSalary { get; set; }
        public decimal? NewSalary { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Reward DTOs
    public class CreateRewardDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public RewardType Type { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public decimal? Amount { get; set; }

        [Required]
        public DateTime RewardDate { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }
    }

    public class RewardDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public RewardType Type { get; set; }
        public string? Description { get; set; }
        public decimal? Amount { get; set; }
        public DateTime RewardDate { get; set; }
        public string? Reason { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? DocumentPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Discipline DTOs
    public class CreateDisciplineDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DisciplineType Type { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public DateTime ViolationDate { get; set; }

        [Required]
        public DateTime DisciplineDate { get; set; }

        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal? FinePenalty { get; set; }
        public int? SuspensionDays { get; set; }
    }

    public class DisciplineDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DisciplineType Type { get; set; }
        public string? Description { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime ViolationDate { get; set; }
        public DateTime DisciplineDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal? FinePenalty { get; set; }
        public int? SuspensionDays { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? DocumentPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Document DTOs
    public class UploadDocumentDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DocumentType Type { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public IFormFile File { get; set; } = null!;
    }

    public class DocumentDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public long FileSize { get; set; }
        public string? Description { get; set; }
        public string? UploadedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // File Response DTO
    public class FileResponseDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public Stream FileStream { get; set; } = null!;
    }
}
