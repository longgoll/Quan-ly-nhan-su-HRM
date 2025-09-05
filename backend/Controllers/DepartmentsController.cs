using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using backend.Services;
using backend.DTOs;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(IEmployeeService employeeService, ILogger<DepartmentsController> logger)
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
        public async Task<ActionResult<List<DepartmentDto>>> GetAllDepartments()
        {
            try
            {
                var departments = await _employeeService.GetAllDepartmentsAsync();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all departments");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
        {
            try
            {
                var department = await _employeeService.GetDepartmentByIdAsync(id);
                if (department == null)
                {
                    return NotFound();
                }
                return Ok(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting department {DepartmentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment(CreateDepartmentDto createDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var department = await _employeeService.CreateDepartmentAsync(createDto, currentUserId);
                return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, department);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<ActionResult<DepartmentDto>> UpdateDepartment(int id, UpdateDepartmentDto updateDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var department = await _employeeService.UpdateDepartmentAsync(id, updateDto, currentUserId);
                return Ok(department);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department {DepartmentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _employeeService.DeleteDepartmentAsync(id, currentUserId);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting department {DepartmentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
