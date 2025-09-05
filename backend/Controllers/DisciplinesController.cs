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
    public class DisciplinesController : ControllerBase
    {
        private readonly IDisciplineService _disciplineService;
        private readonly IMinIOService _minIOService;
        private readonly ILogger<DisciplinesController> _logger;

        public DisciplinesController(
            IDisciplineService disciplineService,
            IMinIOService minIOService,
            ILogger<DisciplinesController> logger)
        {
            _disciplineService = disciplineService;
            _minIOService = minIOService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet]
        public async Task<ActionResult<List<DisciplineDto>>> GetAllDisciplines()
        {
            try
            {
                var disciplines = await _disciplineService.GetAllDisciplinesAsync();
                return Ok(disciplines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all disciplines");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DisciplineDto>> GetDiscipline(int id)
        {
            try
            {
                var discipline = await _disciplineService.GetDisciplineByIdAsync(id);
                if (discipline == null)
                {
                    return NotFound();
                }
                return Ok(discipline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting discipline {DisciplineId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<List<DisciplineDto>>> GetDisciplinesByEmployee(int employeeId)
        {
            try
            {
                var disciplines = await _disciplineService.GetDisciplinesByEmployeeIdAsync(employeeId);
                return Ok(disciplines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting disciplines for employee {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<ActionResult<DisciplineDto>> CreateDiscipline(CreateDisciplineDto createDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var discipline = await _disciplineService.CreateDisciplineAsync(createDto, currentUserId);
                return CreatedAtAction(nameof(GetDiscipline), new { id = discipline.Id }, discipline);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating discipline");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> ApproveDiscipline(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _disciplineService.ApproveDisciplineAsync(id, currentUserId);
                if (!result)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving discipline {DisciplineId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/upload-document")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> UploadDisciplineDocument(int id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("File is required");
                }

                var allowedTypes = new[] { "pdf", "doc", "docx", "jpg", "jpeg", "png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant().TrimStart('.');
                
                if (!allowedTypes.Contains(fileExtension))
                {
                    return BadRequest($"File type .{fileExtension} is not allowed");
                }

                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (file.Length > maxFileSize)
                {
                    return BadRequest("File size cannot exceed 10MB");
                }

                var contentType = fileExtension switch
                {
                    "pdf" => "application/pdf",
                    "doc" => "application/msword",
                    "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "jpg" or "jpeg" => "image/jpeg",
                    "png" => "image/png",
                    _ => "application/octet-stream"
                };

                var folderName = $"disciplines/{id}";
                using var stream = file.OpenReadStream();
                var filePath = await _minIOService.UploadFileAsync(stream, file.FileName, contentType, folderName);

                var currentUserId = GetCurrentUserId();
                var result = await _disciplineService.UploadDisciplineDocumentAsync(id, filePath, currentUserId);
                
                if (!result)
                {
                    return NotFound();
                }

                return Ok(new { filePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading discipline document for discipline {DisciplineId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> DeleteDiscipline(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _disciplineService.DeleteDisciplineAsync(id, currentUserId);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting discipline {DisciplineId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
