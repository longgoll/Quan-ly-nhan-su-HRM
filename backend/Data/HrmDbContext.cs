using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class HrmDbContext : DbContext
    {
        public HrmDbContext(DbContextOptions<HrmDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<EmployeeContract> EmployeeContracts { get; set; }
        public DbSet<WorkHistory> WorkHistories { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        public DbSet<Discipline> Disciplines { get; set; }
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }

        // Work Schedule Management
        public DbSet<WorkShift> WorkShifts { get; set; }
        public DbSet<EmployeeShiftAssignment> EmployeeShiftAssignments { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }

        // Attendance Management
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<AttendanceDetail> AttendanceDetails { get; set; }
        public DbSet<AttendanceSummary> AttendanceSummaries { get; set; }

        // Leave Management
        public DbSet<LeavePolicy> LeavePolicies { get; set; }
        public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveApprovalWorkflow> LeaveApprovalWorkflows { get; set; }
        public DbSet<PublicHoliday> PublicHolidays { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.HasIndex(e => e.Username)
                    .IsUnique();

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(e => e.Role)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Self-referencing relationship for ApprovedBy
                entity.HasOne(e => e.ApprovedBy)
                    .WithMany(e => e.ApprovedUsers)
                    .HasForeignKey(e => e.ApprovedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Employee entity
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.EmployeeCode)
                    .IsUnique()
                    .HasFilter("[EmployeeCode] IS NOT NULL");

                entity.HasIndex(e => e.IdentityNumber)
                    .IsUnique()
                    .HasFilter("[IdentityNumber] IS NOT NULL");

                entity.Property(e => e.Gender)
                    .HasConversion<string>();

                entity.Property(e => e.MaritalStatus)
                    .HasConversion<string>();

                entity.Property(e => e.Status)
                    .HasConversion<string>();

                // One-to-One relationship with User
                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<Employee>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Self-referencing relationship for DirectManager
                entity.HasOne(e => e.DirectManager)
                    .WithMany(e => e.Subordinates)
                    .HasForeignKey(e => e.DirectManagerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Department entity
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Code)
                    .IsUnique()
                    .HasFilter("[Code] IS NOT NULL");

                // Self-referencing relationship for ParentDepartment
                entity.HasOne(e => e.ParentDepartment)
                    .WithMany(e => e.SubDepartments)
                    .HasForeignKey(e => e.ParentDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                // One-to-Many relationship with Manager
                entity.HasOne(e => e.Manager)
                    .WithMany()
                    .HasForeignKey(e => e.ManagerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Position entity
            modelBuilder.Entity<Position>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Code)
                    .IsUnique()
                    .HasFilter("[Code] IS NOT NULL");
            });

            // Configure EmployeeContract entity
            modelBuilder.Entity<EmployeeContract>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.ContractNumber)
                    .IsUnique();

                entity.Property(e => e.Type)
                    .HasConversion<string>();

                entity.Property(e => e.Status)
                    .HasConversion<string>();
            });

            // Configure WorkHistory entity
            modelBuilder.Entity<WorkHistory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Type)
                    .HasConversion<string>();

                // Configure foreign key relationships
                entity.HasOne(e => e.OldDepartment)
                    .WithMany()
                    .HasForeignKey(e => e.OldDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.NewDepartment)
                    .WithMany()
                    .HasForeignKey(e => e.NewDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.OldPosition)
                    .WithMany()
                    .HasForeignKey(e => e.OldPositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.NewPosition)
                    .WithMany()
                    .HasForeignKey(e => e.NewPositionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Reward entity
            modelBuilder.Entity<Reward>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Type)
                    .HasConversion<string>();
            });

            // Configure Discipline entity
            modelBuilder.Entity<Discipline>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Type)
                    .HasConversion<string>();
            });

            // Configure EmployeeDocument entity
            modelBuilder.Entity<EmployeeDocument>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Type)
                    .HasConversion<string>();
            });

            // Configure WorkShift entity
            modelBuilder.Entity<WorkShift>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Code)
                    .IsUnique()
                    .HasFilter("[Code] IS NOT NULL");

                entity.Property(e => e.Type)
                    .HasConversion<string>();

                entity.Property(e => e.Status)
                    .HasConversion<string>();
            });

            // Configure EmployeeShiftAssignment entity
            modelBuilder.Entity<EmployeeShiftAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.EmployeeId, e.WorkShiftId, e.EffectiveFrom })
                    .IsUnique();
            });

            // Configure WorkSchedule entity
            modelBuilder.Entity<WorkSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.EmployeeId, e.WorkDate })
                    .IsUnique();
            });

            // Configure Attendance entity
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.EmployeeId, e.AttendanceDate })
                    .IsUnique();

                entity.Property(e => e.Status)
                    .HasConversion<string>();
            });

            // Configure AttendanceDetail entity
            modelBuilder.Entity<AttendanceDetail>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Type)
                    .HasConversion<string>();
            });

            // Configure AttendanceSummary entity
            modelBuilder.Entity<AttendanceSummary>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.EmployeeId, e.Year, e.Month })
                    .IsUnique();
            });

            // Configure LeavePolicy entity
            modelBuilder.Entity<LeavePolicy>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.LeaveType)
                    .HasConversion<string>();
            });

            // Configure EmployeeLeaveBalance entity
            modelBuilder.Entity<EmployeeLeaveBalance>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.EmployeeId, e.LeavePolicyId, e.Year })
                    .IsUnique();

                entity.Property(e => e.AllocatedDays)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.UsedDays)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.CarriedForwardDays)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.AdjustmentDays)
                    .HasColumnType("decimal(5,2)");
            });

            // Configure LeaveRequest entity
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.Property(e => e.RequestedDays)
                    .HasColumnType("decimal(4,2)");

                entity.HasOne(e => e.CoverEmployee)
                    .WithMany()
                    .HasForeignKey(e => e.CoverEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApprovedBy)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure LeaveApprovalWorkflow entity
            modelBuilder.Entity<LeaveApprovalWorkflow>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasIndex(e => new { e.LeaveRequestId, e.Order })
                    .IsUnique();
            });

            // Configure PublicHoliday entity
            modelBuilder.Entity<PublicHoliday>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Date);
            });
        }
    }
}
