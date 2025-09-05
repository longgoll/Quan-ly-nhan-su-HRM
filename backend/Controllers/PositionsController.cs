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
    public class PositionsController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<PositionsController> _logger;

        public PositionsController(IEmployeeService employeeService, ILogger<PositionsController> logger)
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
        public async Task<ActionResult<List<PositionDto>>> GetAllPositions()
        {
            try
            {
                var positions = await _employeeService.GetAllPositionsAsync();
                return Ok(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all positions");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PositionDto>> GetPosition(int id)
        {
            try
            {
                var position = await _employeeService.GetPositionByIdAsync(id);
                if (position == null)
                {
                    return NotFound();
                }
                return Ok(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting position {PositionId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<List<PositionDto>>> GetPositionsByDepartment(int departmentId)
        {
            try
            {
                var positions = await _employeeService.GetPositionsByDepartmentAsync(departmentId);
                return Ok(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting positions by department {DepartmentId}", departmentId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<ActionResult<PositionDto>> CreatePosition(CreatePositionDto createDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var position = await _employeeService.CreatePositionAsync(createDto, currentUserId);
                return CreatedAtAction(nameof(GetPosition), new { id = position.Id }, position);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating position");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<ActionResult<PositionDto>> UpdatePosition(int id, UpdatePositionDto updateDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var position = await _employeeService.UpdatePositionAsync(id, updateDto, currentUserId);
                return Ok(position);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating position {PositionId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> DeletePosition(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _employeeService.DeletePositionAsync(id, currentUserId);
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
                _logger.LogError(ex, "Error deleting position {PositionId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
