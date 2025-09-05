using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using AutoMapper;

namespace backend.Services
{
    public interface IWorkHistoryService
    {
        Task<List<WorkHistoryDto>> GetAllWorkHistoriesAsync();
        Task<WorkHistoryDto?> GetWorkHistoryByIdAsync(int id);
        Task<List<WorkHistoryDto>> GetWorkHistoriesByEmployeeIdAsync(int employeeId);
        Task<WorkHistoryDto> CreateWorkHistoryAsync(CreateWorkHistoryDto createDto, int createdById);
        Task<bool> ApproveWorkHistoryAsync(int id, int approvedById);
        Task<bool> DeleteWorkHistoryAsync(int id, int deletedById);
    }

    public class WorkHistoryService : IWorkHistoryService
    {
        private readonly HrmDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<WorkHistoryService> _logger;

        public WorkHistoryService(HrmDbContext context, IMapper mapper, ILogger<WorkHistoryService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<WorkHistoryDto>> GetAllWorkHistoriesAsync()
        {
            try
            {
                var workHistories = await _context.WorkHistories
                    .Include(w => w.Employee)
                    .Include(w => w.OldDepartment)
                    .Include(w => w.NewDepartment)
                    .Include(w => w.OldPosition)
                    .Include(w => w.NewPosition)
                    .Include(w => w.ApprovedBy)
                    .OrderByDescending(w => w.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<WorkHistoryDto>>(workHistories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all work histories");
                throw new InvalidOperationException("Failed to retrieve work histories", ex);
            }
        }

        public async Task<WorkHistoryDto?> GetWorkHistoryByIdAsync(int id)
        {
            try
            {
                var workHistory = await _context.WorkHistories
                    .Include(w => w.Employee)
                    .Include(w => w.OldDepartment)
                    .Include(w => w.NewDepartment)
                    .Include(w => w.OldPosition)
                    .Include(w => w.NewPosition)
                    .Include(w => w.ApprovedBy)
                    .FirstOrDefaultAsync(w => w.Id == id);

                return workHistory != null ? _mapper.Map<WorkHistoryDto>(workHistory) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work history by ID: {WorkHistoryId}", id);
                throw new InvalidOperationException($"Failed to retrieve work history with ID {id}", ex);
            }
        }

        public async Task<List<WorkHistoryDto>> GetWorkHistoriesByEmployeeIdAsync(int employeeId)
        {
            try
            {
                var workHistories = await _context.WorkHistories
                    .Include(w => w.Employee)
                    .Include(w => w.OldDepartment)
                    .Include(w => w.NewDepartment)
                    .Include(w => w.OldPosition)
                    .Include(w => w.NewPosition)
                    .Include(w => w.ApprovedBy)
                    .Where(w => w.EmployeeId == employeeId)
                    .OrderByDescending(w => w.EffectiveDate)
                    .ToListAsync();

                return _mapper.Map<List<WorkHistoryDto>>(workHistories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work histories for employee: {EmployeeId}", employeeId);
                throw new InvalidOperationException($"Failed to retrieve work histories for employee {employeeId}", ex);
            }
        }

        public async Task<WorkHistoryDto> CreateWorkHistoryAsync(CreateWorkHistoryDto createDto, int createdById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if employee exists
                var employee = await _context.Employees.FindAsync(createDto.EmployeeId);
                if (employee == null)
                {
                    throw new ArgumentException("Employee not found");
                }

                var workHistory = _mapper.Map<WorkHistory>(createDto);
                workHistory.CreatedAt = DateTime.UtcNow;

                _context.WorkHistories.Add(workHistory);

                // Update employee information if approved automatically
                if (createDto.Type == WorkHistoryType.Transfer && createDto.NewDepartmentId.HasValue)
                {
                    employee.DepartmentId = createDto.NewDepartmentId;
                }
                if (createDto.Type == WorkHistoryType.Promotion && createDto.NewPositionId.HasValue)
                {
                    employee.PositionId = createDto.NewPositionId;
                }
                if (createDto.Type == WorkHistoryType.SalaryIncrease && createDto.NewSalary.HasValue)
                {
                    employee.BaseSalary = createDto.NewSalary;
                }

                employee.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Work history created successfully: {WorkHistoryId} by user {UserId}", workHistory.Id, createdById);
                return await GetWorkHistoryByIdAsync(workHistory.Id) ?? throw new InvalidOperationException("Failed to retrieve created work history");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating work history for employee {EmployeeId}", createDto.EmployeeId);
                throw;
            }
        }

        public async Task<bool> ApproveWorkHistoryAsync(int id, int approvedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var workHistory = await _context.WorkHistories
                    .Include(w => w.Employee)
                    .FirstOrDefaultAsync(w => w.Id == id);

                if (workHistory == null)
                {
                    return false;
                }

                workHistory.ApprovedById = approvedById;
                workHistory.ApprovedDate = DateTime.UtcNow;

                // Apply changes to employee record
                var employee = workHistory.Employee;
                if (workHistory.Type == WorkHistoryType.Transfer && workHistory.NewDepartmentId.HasValue)
                {
                    employee.DepartmentId = workHistory.NewDepartmentId;
                }
                if (workHistory.Type == WorkHistoryType.Promotion && workHistory.NewPositionId.HasValue)
                {
                    employee.PositionId = workHistory.NewPositionId;
                }
                if (workHistory.Type == WorkHistoryType.SalaryIncrease && workHistory.NewSalary.HasValue)
                {
                    employee.BaseSalary = workHistory.NewSalary;
                }

                employee.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Work history approved successfully: {WorkHistoryId} by user {UserId}", id, approvedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error approving work history {WorkHistoryId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteWorkHistoryAsync(int id, int deletedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var workHistory = await _context.WorkHistories.FindAsync(id);
                if (workHistory == null)
                {
                    return false;
                }

                _context.WorkHistories.Remove(workHistory);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Work history deleted successfully: {WorkHistoryId} by user {UserId}", id, deletedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting work history {WorkHistoryId}", id);
                return false;
            }
        }
    }
}
