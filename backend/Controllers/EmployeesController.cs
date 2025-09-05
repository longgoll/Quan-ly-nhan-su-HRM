using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using backend.Services;
using backend.DTOs;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet]
        public async Task<ActionResult<List<EmployeeDto>>> GetAllEmployees()
        {
            try
            {
                var employees = await _employeeService.GetAllEmployeesAsync();
                return Ok(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all employees");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    return NotFound();
                }
                return Ok(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee {EmployeeId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployeeByUserId(int userId)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(userId);
                if (employee == null)
                {
                    return NotFound();
                }
                return Ok(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee by user ID {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee(CreateEmployeeDto createDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var employee = await _employeeService.CreateEmployeeAsync(createDto, currentUserId);
                return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<ActionResult<EmployeeDto>> UpdateEmployee(int id, UpdateEmployeeDto updateDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var employee = await _employeeService.UpdateEmployeeAsync(id, updateDto, currentUserId);
                return Ok(employee);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee {EmployeeId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _employeeService.DeleteEmployeeAsync(id, currentUserId);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee {EmployeeId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<List<EmployeeBasicDto>>> GetEmployeesByDepartment(int departmentId)
        {
            try
            {
                var employees = await _employeeService.GetEmployeesByDepartmentAsync(departmentId);
                return Ok(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employees by department {DepartmentId}", departmentId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("manager/{managerId}/subordinates")]
        public async Task<ActionResult<List<EmployeeBasicDto>>> GetSubordinates(int managerId)
        {
            try
            {
                var subordinates = await _employeeService.GetSubordinatesAsync(managerId);
                return Ok(subordinates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subordinates for manager {ManagerId}", managerId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{employeeId}/assign-manager/{managerId}")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> AssignManager(int employeeId, int managerId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _employeeService.AssignManagerAsync(employeeId, managerId, currentUserId);
                if (!result)
                {
                    return BadRequest("Failed to assign manager");
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning manager {ManagerId} to employee {EmployeeId}", managerId, employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("generate-code")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<ActionResult<string>> GenerateEmployeeCode()
        {
            try
            {
                var code = await _employeeService.GenerateEmployeeCodeAsync();
                return Ok(new { code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating employee code");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
