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
    public class WorkHistoryController : ControllerBase
    {
        private readonly IWorkHistoryService _workHistoryService;
        private readonly ILogger<WorkHistoryController> _logger;

        public WorkHistoryController(IWorkHistoryService workHistoryService, ILogger<WorkHistoryController> logger)
        {
            _workHistoryService = workHistoryService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet]
        public async Task<ActionResult<List<WorkHistoryDto>>> GetAllWorkHistories()
        {
            try
            {
                var workHistories = await _workHistoryService.GetAllWorkHistoriesAsync();
                return Ok(workHistories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all work histories");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WorkHistoryDto>> GetWorkHistory(int id)
        {
            try
            {
                var workHistory = await _workHistoryService.GetWorkHistoryByIdAsync(id);
                if (workHistory == null)
                {
                    return NotFound();
                }
                return Ok(workHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work history {WorkHistoryId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<List<WorkHistoryDto>>> GetWorkHistoriesByEmployee(int employeeId)
        {
            try
            {
                var workHistories = await _workHistoryService.GetWorkHistoriesByEmployeeIdAsync(employeeId);
                return Ok(workHistories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work histories for employee {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<ActionResult<WorkHistoryDto>> CreateWorkHistory(CreateWorkHistoryDto createDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var workHistory = await _workHistoryService.CreateWorkHistoryAsync(createDto, currentUserId);
                return CreatedAtAction(nameof(GetWorkHistory), new { id = workHistory.Id }, workHistory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work history");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> ApproveWorkHistory(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _workHistoryService.ApproveWorkHistoryAsync(id, currentUserId);
                if (!result)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving work history {WorkHistoryId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> DeleteWorkHistory(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _workHistoryService.DeleteWorkHistoryAsync(id, currentUserId);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting work history {WorkHistoryId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
