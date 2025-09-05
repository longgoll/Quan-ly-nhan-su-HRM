using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using AutoMapper;

namespace backend.Services
{
    public interface IRewardService
    {
        Task<List<RewardDto>> GetAllRewardsAsync();
        Task<RewardDto?> GetRewardByIdAsync(int id);
        Task<List<RewardDto>> GetRewardsByEmployeeIdAsync(int employeeId);
        Task<RewardDto> CreateRewardAsync(CreateRewardDto createDto, int createdById);
        Task<bool> ApproveRewardAsync(int id, int approvedById);
        Task<bool> UploadRewardDocumentAsync(int rewardId, string filePath, int uploadedById);
        Task<bool> DeleteRewardAsync(int id, int deletedById);
    }

    public class RewardService : IRewardService
    {
        private readonly HrmDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<RewardService> _logger;

        public RewardService(HrmDbContext context, IMapper mapper, ILogger<RewardService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<RewardDto>> GetAllRewardsAsync()
        {
            try
            {
                var rewards = await _context.Rewards
                    .Include(r => r.Employee)
                    .Include(r => r.ApprovedBy)
                    .OrderByDescending(r => r.RewardDate)
                    .ToListAsync();

                return _mapper.Map<List<RewardDto>>(rewards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all rewards");
                throw new InvalidOperationException("Failed to retrieve rewards", ex);
            }
        }

        public async Task<RewardDto?> GetRewardByIdAsync(int id)
        {
            try
            {
                var reward = await _context.Rewards
                    .Include(r => r.Employee)
                    .Include(r => r.ApprovedBy)
                    .FirstOrDefaultAsync(r => r.Id == id);

                return reward != null ? _mapper.Map<RewardDto>(reward) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reward by ID: {RewardId}", id);
                throw new InvalidOperationException($"Failed to retrieve reward with ID {id}", ex);
            }
        }

        public async Task<List<RewardDto>> GetRewardsByEmployeeIdAsync(int employeeId)
        {
            try
            {
                var rewards = await _context.Rewards
                    .Include(r => r.Employee)
                    .Include(r => r.ApprovedBy)
                    .Where(r => r.EmployeeId == employeeId)
                    .OrderByDescending(r => r.RewardDate)
                    .ToListAsync();

                return _mapper.Map<List<RewardDto>>(rewards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rewards for employee: {EmployeeId}", employeeId);
                throw new InvalidOperationException($"Failed to retrieve rewards for employee {employeeId}", ex);
            }
        }

        public async Task<RewardDto> CreateRewardAsync(CreateRewardDto createDto, int createdById)
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

                var reward = _mapper.Map<Reward>(createDto);
                reward.CreatedAt = DateTime.UtcNow;

                _context.Rewards.Add(reward);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Reward created successfully: {RewardId} by user {UserId}", reward.Id, createdById);
                return await GetRewardByIdAsync(reward.Id) ?? throw new InvalidOperationException("Failed to retrieve created reward");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating reward for employee {EmployeeId}", createDto.EmployeeId);
                throw;
            }
        }

        public async Task<bool> ApproveRewardAsync(int id, int approvedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var reward = await _context.Rewards.FindAsync(id);
                if (reward == null)
                {
                    return false;
                }

                reward.ApprovedById = approvedById;
                reward.ApprovedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Reward approved successfully: {RewardId} by user {UserId}", id, approvedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error approving reward {RewardId}", id);
                return false;
            }
        }

        public async Task<bool> UploadRewardDocumentAsync(int rewardId, string filePath, int uploadedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var reward = await _context.Rewards.FindAsync(rewardId);
                if (reward == null)
                {
                    return false;
                }

                reward.DocumentPath = filePath;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Reward document uploaded successfully: {RewardId} by user {UserId}", rewardId, uploadedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error uploading document for reward {RewardId}", rewardId);
                return false;
            }
        }

        public async Task<bool> DeleteRewardAsync(int id, int deletedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var reward = await _context.Rewards.FindAsync(id);
                if (reward == null)
                {
                    return false;
                }

                _context.Rewards.Remove(reward);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Reward deleted successfully: {RewardId} by user {UserId}", id, deletedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting reward {RewardId}", id);
                return false;
            }
        }
    }

    public interface IDisciplineService
    {
        Task<List<DisciplineDto>> GetAllDisciplinesAsync();
        Task<DisciplineDto?> GetDisciplineByIdAsync(int id);
        Task<List<DisciplineDto>> GetDisciplinesByEmployeeIdAsync(int employeeId);
        Task<DisciplineDto> CreateDisciplineAsync(CreateDisciplineDto createDto, int createdById);
        Task<bool> ApproveDisciplineAsync(int id, int approvedById);
        Task<bool> UploadDisciplineDocumentAsync(int disciplineId, string filePath, int uploadedById);
        Task<bool> DeleteDisciplineAsync(int id, int deletedById);
    }

    public class DisciplineService : IDisciplineService
    {
        private readonly HrmDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DisciplineService> _logger;

        public DisciplineService(HrmDbContext context, IMapper mapper, ILogger<DisciplineService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<DisciplineDto>> GetAllDisciplinesAsync()
        {
            try
            {
                var disciplines = await _context.Disciplines
                    .Include(d => d.Employee)
                    .Include(d => d.ApprovedBy)
                    .OrderByDescending(d => d.DisciplineDate)
                    .ToListAsync();

                return _mapper.Map<List<DisciplineDto>>(disciplines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all disciplines");
                throw new InvalidOperationException("Failed to retrieve disciplines", ex);
            }
        }

        public async Task<DisciplineDto?> GetDisciplineByIdAsync(int id)
        {
            try
            {
                var discipline = await _context.Disciplines
                    .Include(d => d.Employee)
                    .Include(d => d.ApprovedBy)
                    .FirstOrDefaultAsync(d => d.Id == id);

                return discipline != null ? _mapper.Map<DisciplineDto>(discipline) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting discipline by ID: {DisciplineId}", id);
                throw new InvalidOperationException($"Failed to retrieve discipline with ID {id}", ex);
            }
        }

        public async Task<List<DisciplineDto>> GetDisciplinesByEmployeeIdAsync(int employeeId)
        {
            try
            {
                var disciplines = await _context.Disciplines
                    .Include(d => d.Employee)
                    .Include(d => d.ApprovedBy)
                    .Where(d => d.EmployeeId == employeeId)
                    .OrderByDescending(d => d.DisciplineDate)
                    .ToListAsync();

                return _mapper.Map<List<DisciplineDto>>(disciplines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting disciplines for employee: {EmployeeId}", employeeId);
                throw new InvalidOperationException($"Failed to retrieve disciplines for employee {employeeId}", ex);
            }
        }

        public async Task<DisciplineDto> CreateDisciplineAsync(CreateDisciplineDto createDto, int createdById)
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

                var discipline = _mapper.Map<Discipline>(createDto);
                discipline.CreatedAt = DateTime.UtcNow;

                _context.Disciplines.Add(discipline);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Discipline created successfully: {DisciplineId} by user {UserId}", discipline.Id, createdById);
                return await GetDisciplineByIdAsync(discipline.Id) ?? throw new InvalidOperationException("Failed to retrieve created discipline");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating discipline for employee {EmployeeId}", createDto.EmployeeId);
                throw;
            }
        }

        public async Task<bool> ApproveDisciplineAsync(int id, int approvedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var discipline = await _context.Disciplines.FindAsync(id);
                if (discipline == null)
                {
                    return false;
                }

                discipline.ApprovedById = approvedById;
                discipline.ApprovedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Discipline approved successfully: {DisciplineId} by user {UserId}", id, approvedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error approving discipline {DisciplineId}", id);
                return false;
            }
        }

        public async Task<bool> UploadDisciplineDocumentAsync(int disciplineId, string filePath, int uploadedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var discipline = await _context.Disciplines.FindAsync(disciplineId);
                if (discipline == null)
                {
                    return false;
                }

                discipline.DocumentPath = filePath;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Discipline document uploaded successfully: {DisciplineId} by user {UserId}", disciplineId, uploadedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error uploading document for discipline {DisciplineId}", disciplineId);
                return false;
            }
        }

        public async Task<bool> DeleteDisciplineAsync(int id, int deletedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var discipline = await _context.Disciplines.FindAsync(id);
                if (discipline == null)
                {
                    return false;
                }

                _context.Disciplines.Remove(discipline);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Discipline deleted successfully: {DisciplineId} by user {UserId}", id, deletedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting discipline {DisciplineId}", id);
                return false;
            }
        }
    }
}
