using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Employee;

        public bool IsActive { get; set; } = true;

        public bool IsApproved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public int? ApprovedById { get; set; }
        [ForeignKey("ApprovedById")]
        public User? ApprovedBy { get; set; }

        public virtual ICollection<User> ApprovedUsers { get; set; } = new List<User>();
    }

    public enum UserRole
    {
        Employee = 1,
        Manager = 2,
        HRManager = 3,
        Admin = 4
    }
}
