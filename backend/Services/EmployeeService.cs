using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using AutoMapper;

namespace backend.Services
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetAllEmployeesAsync();
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<EmployeeDto?> GetEmployeeByUserIdAsync(int userId);
        Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createDto, int createdById);
        Task<EmployeeDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto updateDto, int updatedById);
        Task<bool> DeleteEmployeeAsync(int id, int deletedById);
        Task<List<EmployeeBasicDto>> GetEmployeesByDepartmentAsync(int departmentId);
        Task<List<EmployeeBasicDto>> GetSubordinatesAsync(int managerId);
        Task<bool> AssignManagerAsync(int employeeId, int managerId, int assignedById);
        Task<string> GenerateEmployeeCodeAsync();

        // Department methods
        Task<List<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto?> GetDepartmentByIdAsync(int id);
        Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto createDto, int createdById);
        Task<DepartmentDto> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDto, int updatedById);
        Task<bool> DeleteDepartmentAsync(int id, int deletedById);

        // Position methods
        Task<List<PositionDto>> GetAllPositionsAsync();
        Task<PositionDto?> GetPositionByIdAsync(int id);
        Task<PositionDto> CreatePositionAsync(CreatePositionDto createDto, int createdById);
        Task<PositionDto> UpdatePositionAsync(int id, UpdatePositionDto updateDto, int updatedById);
        Task<bool> DeletePositionAsync(int id, int deletedById);
        Task<List<PositionDto>> GetPositionsByDepartmentAsync(int departmentId);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly HrmDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(HrmDbContext context, IMapper mapper, ILogger<EmployeeService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        #region Employee Methods

        public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
        {
            try
            {
                var employees = await _context.Employees
                    .Include(e => e.User)
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Include(e => e.DirectManager)
                        .ThenInclude(m => m!.User)
                    .OrderBy(e => e.EmployeeCode)
                    .ToListAsync();

                return _mapper.Map<List<EmployeeDto>>(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all employees");
                throw new InvalidOperationException("Failed to retrieve employees", ex);
            }
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            try
            {
                var employee = await _context.Employees
                    .Include(e => e.User)
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Include(e => e.DirectManager)
                        .ThenInclude(m => m!.User)
                    .FirstOrDefaultAsync(e => e.Id == id);

                return employee != null ? _mapper.Map<EmployeeDto>(employee) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee by ID: {EmployeeId}", id);
                throw new InvalidOperationException($"Failed to retrieve employee with ID {id}", ex);
            }
        }

        public async Task<EmployeeDto?> GetEmployeeByUserIdAsync(int userId)
        {
            try
            {
                var employee = await _context.Employees
                    .Include(e => e.User)
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Include(e => e.DirectManager)
                        .ThenInclude(m => m!.User)
                    .FirstOrDefaultAsync(e => e.UserId == userId);

                return employee != null ? _mapper.Map<EmployeeDto>(employee) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee by user ID: {UserId}", userId);
                throw new InvalidOperationException($"Failed to retrieve employee with user ID {userId}", ex);
            }
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createDto, int createdById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if user exists and doesn't already have an employee record
                var user = await _context.Users.FindAsync(createDto.UserId);
                if (user == null)
                {
                    throw new ArgumentException("User not found");
                }

                var existingEmployee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.UserId == createDto.UserId);
                if (existingEmployee != null)
                {
                    throw new ArgumentException("Employee record already exists for this user");
                }

                // Generate employee code if not provided
                var employeeCode = !string.IsNullOrEmpty(createDto.EmployeeCode) 
                    ? createDto.EmployeeCode 
                    : await GenerateEmployeeCodeAsync();

                // Check if employee code is unique
                var codeExists = await _context.Employees
                    .AnyAsync(e => e.EmployeeCode == employeeCode);
                if (codeExists)
                {
                    throw new ArgumentException("Employee code already exists");
                }

                var employee = _mapper.Map<Employee>(createDto);
                employee.EmployeeCode = employeeCode;
                employee.CreatedAt = DateTime.UtcNow;

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Employee created successfully: {EmployeeId} by user {UserId}", employee.Id, createdById);
                return await GetEmployeeByIdAsync(employee.Id) ?? throw new InvalidOperationException("Failed to retrieve created employee");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating employee for user {UserId}", createDto.UserId);
                throw;
            }
        }

        public async Task<EmployeeDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto updateDto, int updatedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                {
                    throw new ArgumentException("Employee not found");
                }

                _mapper.Map(updateDto, employee);
                employee.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Employee updated successfully: {EmployeeId} by user {UserId}", id, updatedById);
                return await GetEmployeeByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated employee");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating employee {EmployeeId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteEmployeeAsync(int id, int deletedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                {
                    return false;
                }

                employee.Status = EmployeeStatus.Terminated;
                employee.TerminationDate = DateTime.UtcNow;
                employee.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Employee deleted successfully: {EmployeeId} by user {UserId}", id, deletedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting employee {EmployeeId}", id);
                return false;
            }
        }

        public async Task<List<EmployeeBasicDto>> GetEmployeesByDepartmentAsync(int departmentId)
        {
            try
            {
                var employees = await _context.Employees
                    .Include(e => e.User)
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Where(e => e.DepartmentId == departmentId && e.Status == EmployeeStatus.Active)
                    .OrderBy(e => e.EmployeeCode)
                    .ToListAsync();

                return _mapper.Map<List<EmployeeBasicDto>>(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employees by department: {DepartmentId}", departmentId);
                throw new InvalidOperationException($"Failed to retrieve employees for department {departmentId}", ex);
            }
        }

        public async Task<List<EmployeeBasicDto>> GetSubordinatesAsync(int managerId)
        {
            try
            {
                var subordinates = await _context.Employees
                    .Include(e => e.User)
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Where(e => e.DirectManagerId == managerId && e.Status == EmployeeStatus.Active)
                    .OrderBy(e => e.EmployeeCode)
                    .ToListAsync();

                return _mapper.Map<List<EmployeeBasicDto>>(subordinates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subordinates for manager: {ManagerId}", managerId);
                throw new InvalidOperationException($"Failed to retrieve subordinates for manager {managerId}", ex);
            }
        }

        public async Task<bool> AssignManagerAsync(int employeeId, int managerId, int assignedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                var manager = await _context.Employees.FindAsync(managerId);

                if (employee == null || manager == null)
                {
                    return false;
                }

                employee.DirectManagerId = managerId;
                employee.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Manager assigned successfully: Employee {EmployeeId} -> Manager {ManagerId} by user {UserId}", 
                    employeeId, managerId, assignedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error assigning manager {ManagerId} to employee {EmployeeId}", managerId, employeeId);
                return false;
            }
        }

        public async Task<string> GenerateEmployeeCodeAsync()
        {
            try
            {
                var currentYear = DateTime.UtcNow.Year;
                var prefix = $"EMP{currentYear}";

                var lastEmployee = await _context.Employees
                    .Where(e => e.EmployeeCode != null && e.EmployeeCode.StartsWith(prefix))
                    .OrderByDescending(e => e.EmployeeCode)
                    .FirstOrDefaultAsync();

                if (lastEmployee == null)
                {
                    return $"{prefix}001";
                }

                var lastCodeNumber = lastEmployee.EmployeeCode![prefix.Length..];
                if (int.TryParse(lastCodeNumber, out int number))
                {
                    return $"{prefix}{(number + 1):D3}";
                }

                return $"{prefix}001";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating employee code");
                throw new InvalidOperationException("Failed to generate employee code", ex);
            }
        }

        #endregion

        #region Department Methods

        public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
        {
            try
            {
                var departments = await _context.Departments
                    .Include(d => d.ParentDepartment)
                    .Include(d => d.Manager)
                        .ThenInclude(m => m!.User)
                    .Include(d => d.Employees)
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                return _mapper.Map<List<DepartmentDto>>(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all departments");
                throw new InvalidOperationException("Failed to retrieve departments", ex);
            }
        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
        {
            try
            {
                var department = await _context.Departments
                    .Include(d => d.ParentDepartment)
                    .Include(d => d.Manager)
                        .ThenInclude(m => m!.User)
                    .Include(d => d.SubDepartments)
                    .Include(d => d.Employees)
                    .FirstOrDefaultAsync(d => d.Id == id);

                return department != null ? _mapper.Map<DepartmentDto>(department) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting department by ID: {DepartmentId}", id);
                throw new InvalidOperationException($"Failed to retrieve department with ID {id}", ex);
            }
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto createDto, int createdById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if code is unique
                if (!string.IsNullOrEmpty(createDto.Code))
                {
                    var codeExists = await _context.Departments
                        .AnyAsync(d => d.Code == createDto.Code);
                    if (codeExists)
                    {
                        throw new ArgumentException("Department code already exists");
                    }
                }

                var department = _mapper.Map<Department>(createDto);
                department.CreatedAt = DateTime.UtcNow;

                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Department created successfully: {DepartmentId} by user {UserId}", department.Id, createdById);
                return await GetDepartmentByIdAsync(department.Id) ?? throw new InvalidOperationException("Failed to retrieve created department");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating department {DepartmentName}", createDto.Name);
                throw;
            }
        }

        public async Task<DepartmentDto> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDto, int updatedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var department = await _context.Departments.FindAsync(id);
                if (department == null)
                {
                    throw new ArgumentException("Department not found");
                }

                // Check if code is unique (excluding current department)
                if (!string.IsNullOrEmpty(updateDto.Code) && updateDto.Code != department.Code)
                {
                    var codeExists = await _context.Departments
                        .AnyAsync(d => d.Code == updateDto.Code && d.Id != id);
                    if (codeExists)
                    {
                        throw new ArgumentException("Department code already exists");
                    }
                }

                _mapper.Map(updateDto, department);
                department.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Department updated successfully: {DepartmentId} by user {UserId}", id, updatedById);
                return await GetDepartmentByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated department");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating department {DepartmentId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteDepartmentAsync(int id, int deletedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var department = await _context.Departments
                    .Include(d => d.Employees)
                    .Include(d => d.SubDepartments)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (department == null)
                {
                    return false;
                }

                // Check if department has employees or sub-departments
                if (department.Employees.Any() || department.SubDepartments.Any())
                {
                    throw new InvalidOperationException("Cannot delete department with employees or sub-departments");
                }

                department.IsActive = false;
                department.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Department deleted successfully: {DepartmentId} by user {UserId}", id, deletedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting department {DepartmentId}", id);
                throw;
            }
        }

        #endregion

        #region Position Methods

        public async Task<List<PositionDto>> GetAllPositionsAsync()
        {
            try
            {
                var positions = await _context.Positions
                    .Include(p => p.Department)
                    .Include(p => p.Employees)
                    .OrderBy(p => p.Title)
                    .ToListAsync();

                return _mapper.Map<List<PositionDto>>(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all positions");
                throw new InvalidOperationException("Failed to retrieve positions", ex);
            }
        }

        public async Task<PositionDto?> GetPositionByIdAsync(int id)
        {
            try
            {
                var position = await _context.Positions
                    .Include(p => p.Department)
                    .Include(p => p.Employees)
                    .FirstOrDefaultAsync(p => p.Id == id);

                return position != null ? _mapper.Map<PositionDto>(position) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting position by ID: {PositionId}", id);
                throw new InvalidOperationException($"Failed to retrieve position with ID {id}", ex);
            }
        }

        public async Task<PositionDto> CreatePositionAsync(CreatePositionDto createDto, int createdById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if code is unique
                if (!string.IsNullOrEmpty(createDto.Code))
                {
                    var codeExists = await _context.Positions
                        .AnyAsync(p => p.Code == createDto.Code);
                    if (codeExists)
                    {
                        throw new ArgumentException("Position code already exists");
                    }
                }

                var position = _mapper.Map<Position>(createDto);
                position.CreatedAt = DateTime.UtcNow;

                _context.Positions.Add(position);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Position created successfully: {PositionId} by user {UserId}", position.Id, createdById);
                return await GetPositionByIdAsync(position.Id) ?? throw new InvalidOperationException("Failed to retrieve created position");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating position {PositionTitle}", createDto.Title);
                throw;
            }
        }

        public async Task<PositionDto> UpdatePositionAsync(int id, UpdatePositionDto updateDto, int updatedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var position = await _context.Positions.FindAsync(id);
                if (position == null)
                {
                    throw new ArgumentException("Position not found");
                }

                // Check if code is unique (excluding current position)
                if (!string.IsNullOrEmpty(updateDto.Code) && updateDto.Code != position.Code)
                {
                    var codeExists = await _context.Positions
                        .AnyAsync(p => p.Code == updateDto.Code && p.Id != id);
                    if (codeExists)
                    {
                        throw new ArgumentException("Position code already exists");
                    }
                }

                _mapper.Map(updateDto, position);
                position.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Position updated successfully: {PositionId} by user {UserId}", id, updatedById);
                return await GetPositionByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated position");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating position {PositionId}", id);
                throw;
            }
        }

        public async Task<bool> DeletePositionAsync(int id, int deletedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var position = await _context.Positions
                    .Include(p => p.Employees)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (position == null)
                {
                    return false;
                }

                // Check if position has employees
                if (position.Employees.Any())
                {
                    throw new InvalidOperationException("Cannot delete position with employees");
                }

                position.IsActive = false;
                position.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Position deleted successfully: {PositionId} by user {UserId}", id, deletedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting position {PositionId}", id);
                throw;
            }
        }

        public async Task<List<PositionDto>> GetPositionsByDepartmentAsync(int departmentId)
        {
            try
            {
                var positions = await _context.Positions
                    .Include(p => p.Department)
                    .Include(p => p.Employees)
                    .Where(p => p.DepartmentId == departmentId && p.IsActive)
                    .OrderBy(p => p.Title)
                    .ToListAsync();

                return _mapper.Map<List<PositionDto>>(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting positions by department: {DepartmentId}", departmentId);
                throw new InvalidOperationException($"Failed to retrieve positions for department {departmentId}", ex);
            }
        }

        #endregion
    }
}
