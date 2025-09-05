using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using AutoMapper;

namespace backend.Services
{
    public interface IContractService
    {
        Task<List<ContractDto>> GetAllContractsAsync();
        Task<ContractDto?> GetContractByIdAsync(int id);
        Task<List<ContractDto>> GetContractsByEmployeeIdAsync(int employeeId);
        Task<ContractDto> CreateContractAsync(CreateContractDto createDto, int createdById);
        Task<ContractDto> UpdateContractAsync(int id, UpdateContractDto updateDto, int updatedById);
        Task<bool> TerminateContractAsync(int id, string reason, int terminatedById);
        Task<bool> UploadContractDocumentAsync(int contractId, string filePath, int uploadedById);
    }

    public class ContractService : IContractService
    {
        private readonly HrmDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ContractService> _logger;

        public ContractService(HrmDbContext context, IMapper mapper, ILogger<ContractService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<ContractDto>> GetAllContractsAsync()
        {
            try
            {
                var contracts = await _context.EmployeeContracts
                    .Include(c => c.Employee)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<ContractDto>>(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all contracts");
                throw new InvalidOperationException("Failed to retrieve contracts", ex);
            }
        }

        public async Task<ContractDto?> GetContractByIdAsync(int id)
        {
            try
            {
                var contract = await _context.EmployeeContracts
                    .Include(c => c.Employee)
                    .FirstOrDefaultAsync(c => c.Id == id);

                return contract != null ? _mapper.Map<ContractDto>(contract) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contract by ID: {ContractId}", id);
                throw new InvalidOperationException($"Failed to retrieve contract with ID {id}", ex);
            }
        }

        public async Task<List<ContractDto>> GetContractsByEmployeeIdAsync(int employeeId)
        {
            try
            {
                var contracts = await _context.EmployeeContracts
                    .Include(c => c.Employee)
                    .Where(c => c.EmployeeId == employeeId)
                    .OrderByDescending(c => c.StartDate)
                    .ToListAsync();

                return _mapper.Map<List<ContractDto>>(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contracts for employee: {EmployeeId}", employeeId);
                throw new InvalidOperationException($"Failed to retrieve contracts for employee {employeeId}", ex);
            }
        }

        public async Task<ContractDto> CreateContractAsync(CreateContractDto createDto, int createdById)
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

                // Check if contract number is unique
                var contractExists = await _context.EmployeeContracts
                    .AnyAsync(c => c.ContractNumber == createDto.ContractNumber);
                if (contractExists)
                {
                    throw new ArgumentException("Contract number already exists");
                }

                var contract = _mapper.Map<EmployeeContract>(createDto);
                contract.CreatedAt = DateTime.UtcNow;

                _context.EmployeeContracts.Add(contract);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Contract created successfully: {ContractId} by user {UserId}", contract.Id, createdById);
                return await GetContractByIdAsync(contract.Id) ?? throw new InvalidOperationException("Failed to retrieve created contract");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating contract for employee {EmployeeId}", createDto.EmployeeId);
                throw;
            }
        }

        public async Task<ContractDto> UpdateContractAsync(int id, UpdateContractDto updateDto, int updatedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var contract = await _context.EmployeeContracts.FindAsync(id);
                if (contract == null)
                {
                    throw new ArgumentException("Contract not found");
                }

                // Check if contract number is unique (excluding current contract)
                if (updateDto.ContractNumber != contract.ContractNumber)
                {
                    var contractExists = await _context.EmployeeContracts
                        .AnyAsync(c => c.ContractNumber == updateDto.ContractNumber && c.Id != id);
                    if (contractExists)
                    {
                        throw new ArgumentException("Contract number already exists");
                    }
                }

                _mapper.Map(updateDto, contract);
                contract.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Contract updated successfully: {ContractId} by user {UserId}", id, updatedById);
                return await GetContractByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated contract");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating contract {ContractId}", id);
                throw;
            }
        }

        public async Task<bool> TerminateContractAsync(int id, string reason, int terminatedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var contract = await _context.EmployeeContracts.FindAsync(id);
                if (contract == null)
                {
                    return false;
                }

                contract.Status = ContractStatus.Terminated;
                contract.TerminationReason = reason;
                contract.TerminationDate = DateTime.UtcNow;
                contract.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Contract terminated successfully: {ContractId} by user {UserId}", id, terminatedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error terminating contract {ContractId}", id);
                return false;
            }
        }

        public async Task<bool> UploadContractDocumentAsync(int contractId, string filePath, int uploadedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var contract = await _context.EmployeeContracts.FindAsync(contractId);
                if (contract == null)
                {
                    return false;
                }

                contract.DocumentPath = filePath;
                contract.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Contract document uploaded successfully: {ContractId} by user {UserId}", contractId, uploadedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error uploading document for contract {ContractId}", contractId);
                return false;
            }
        }
    }
}
