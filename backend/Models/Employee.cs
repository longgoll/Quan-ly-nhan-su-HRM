using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    // Model quản lý thông tin nhân viên
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        // Thông tin cá nhân
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        [StringLength(20)]
        public string? IdentityNumber { get; set; } // CMND/CCCD

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

        // Thông tin công việc
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public int? PositionId { get; set; }
        [ForeignKey("PositionId")]
        public Position? Position { get; set; }

        public int? DirectManagerId { get; set; }
        [ForeignKey("DirectManagerId")]
        public Employee? DirectManager { get; set; }

        [StringLength(20)]
        public string? EmployeeCode { get; set; }

        public DateTime HireDate { get; set; }

        public DateTime? TerminationDate { get; set; }

        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? BaseSalary { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
        public virtual ICollection<EmployeeContract> Contracts { get; set; } = new List<EmployeeContract>();
        public virtual ICollection<WorkHistory> WorkHistories { get; set; } = new List<WorkHistory>();
        public virtual ICollection<Reward> Rewards { get; set; } = new List<Reward>();
        public virtual ICollection<Discipline> Disciplines { get; set; } = new List<Discipline>();
        public virtual ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    }

    // Phòng ban
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Code { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int? ParentDepartmentId { get; set; }
        [ForeignKey("ParentDepartmentId")]
        public Department? ParentDepartment { get; set; }

        public int? ManagerId { get; set; }
        [ForeignKey("ManagerId")]
        public Employee? Manager { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Department> SubDepartments { get; set; } = new List<Department>();
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
    }

    // Vị trí công việc
    public class Position
    {
        [Key]
        public int Id { get; set; }

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
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public int Level { get; set; } // Cấp bậc (1-10)

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxSalary { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }

    // Hợp đồng lao động
    public class EmployeeContract
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string ContractNumber { get; set; } = string.Empty;

        public ContractType Type { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        [StringLength(1000)]
        public string? Terms { get; set; }

        public ContractStatus Status { get; set; } = ContractStatus.Active;

        public DateTime? SignedDate { get; set; }

        [StringLength(500)]
        public string? TerminationReason { get; set; }

        public DateTime? TerminationDate { get; set; }

        [StringLength(255)]
        public string? DocumentPath { get; set; } // Đường dẫn file hợp đồng trong MinIO

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    // Lịch sử công tác
    public class WorkHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        public WorkHistoryType Type { get; set; }

        public DateTime EffectiveDate { get; set; }

        public int? OldDepartmentId { get; set; }
        [ForeignKey("OldDepartmentId")]
        public Department? OldDepartment { get; set; }

        public int? NewDepartmentId { get; set; }
        [ForeignKey("NewDepartmentId")]
        public Department? NewDepartment { get; set; }

        public int? OldPositionId { get; set; }
        [ForeignKey("OldPositionId")]
        public Position? OldPosition { get; set; }

        public int? NewPositionId { get; set; }
        [ForeignKey("NewPositionId")]
        public Position? NewPosition { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OldSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? NewSalary { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public int? ApprovedById { get; set; }
        [ForeignKey("ApprovedById")]
        public User? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Khen thưởng
    public class Reward
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public RewardType Type { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; }

        public DateTime RewardDate { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }

        public int? ApprovedById { get; set; }
        [ForeignKey("ApprovedById")]
        public User? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        [StringLength(255)]
        public string? DocumentPath { get; set; } // Đường dẫn file quyết định trong MinIO

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Kỷ luật
    public class Discipline
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public DisciplineType Type { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        public DateTime ViolationDate { get; set; }

        public DateTime DisciplineDate { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? FinePenalty { get; set; }

        public int? SuspensionDays { get; set; }

        public int? ApprovedById { get; set; }
        [ForeignKey("ApprovedById")]
        public User? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        [StringLength(255)]
        public string? DocumentPath { get; set; } // Đường dẫn file quyết định trong MinIO

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Tài liệu nhân viên
    public class EmployeeDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public DocumentType Type { get; set; }

        [Required]
        [StringLength(255)]
        public string FilePath { get; set; } = string.Empty; // Đường dẫn trong MinIO

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? FileType { get; set; }

        public long FileSize { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int? UploadedById { get; set; }
        [ForeignKey("UploadedById")]
        public User? UploadedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Enums
    public enum Gender
    {
        Male = 1,
        Female = 2,
        Other = 3
    }

    public enum MaritalStatus
    {
        Single = 1,
        Married = 2,
        Divorced = 3,
        Widowed = 4
    }

    public enum EmployeeStatus
    {
        Active = 1,
        Inactive = 2,
        Terminated = 3,
        OnLeave = 4,
        Suspended = 5
    }

    public enum ContractType
    {
        Permanent = 1,     // Hợp đồng không thời hạn
        Fixed = 2,         // Hợp đồng có thời hạn
        Probation = 3,     // Hợp đồng thử việc
        Internship = 4,    // Hợp đồng thực tập
        Consultant = 5     // Hợp đồng tư vấn
    }

    public enum ContractStatus
    {
        Draft = 1,
        Active = 2,
        Expired = 3,
        Terminated = 4,
        Renewed = 5
    }

    public enum WorkHistoryType
    {
        Promotion = 1,      // Thăng chức
        Transfer = 2,       // Chuyển phòng ban
        SalaryIncrease = 3, // Tăng lương
        Demotion = 4,       // Giáng chức
        Other = 5
    }

    public enum RewardType
    {
        Achievement = 1,    // Thành tích
        Bonus = 2,         // Thưởng tiền
        Recognition = 3,   // Khen thưởng
        Promotion = 4,     // Thăng chức
        Certificate = 5,   // Bằng khen
        Other = 6
    }

    public enum DisciplineType
    {
        Warning = 1,       // Cảnh cáo
        Reprimand = 2,     // Khiển trách
        Fine = 3,          // Phạt tiền
        Suspension = 4,    // Đình chỉ
        Demotion = 5,      // Giáng chức
        Termination = 6,   // Sa thải
        Other = 7
    }

    public enum DocumentType
    {
        Resume = 1,        // Hồ sơ xin việc
        Contract = 2,      // Hợp đồng
        Identity = 3,      // CMND/CCCD
        Diploma = 4,       // Bằng cấp
        Certificate = 5,   // Chứng chỉ
        Medical = 6,       // Giấy khám sức khỏe
        Background = 7,    // Lý lịch tư pháp
        Photo = 8,         // Ảnh
        Other = 9
    }
}
