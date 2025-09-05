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
    public class RewardsController : ControllerBase
    {
        private readonly IRewardService _rewardService;
        private readonly IMinIOService _minIOService;
        private readonly ILogger<RewardsController> _logger;

        public RewardsController(
            IRewardService rewardService,
            IMinIOService minIOService,
            ILogger<RewardsController> logger)
        {
            _rewardService = rewardService;
            _minIOService = minIOService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet]
        public async Task<ActionResult<List<RewardDto>>> GetAllRewards()
        {
            try
            {
                var rewards = await _rewardService.GetAllRewardsAsync();
                return Ok(rewards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all rewards");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RewardDto>> GetReward(int id)
        {
            try
            {
                var reward = await _rewardService.GetRewardByIdAsync(id);
                if (reward == null)
                {
                    return NotFound();
                }
                return Ok(reward);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reward {RewardId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<List<RewardDto>>> GetRewardsByEmployee(int employeeId)
        {
            try
            {
                var rewards = await _rewardService.GetRewardsByEmployeeIdAsync(employeeId);
                return Ok(rewards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rewards for employee {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<ActionResult<RewardDto>> CreateReward(CreateRewardDto createDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var reward = await _rewardService.CreateRewardAsync(createDto, currentUserId);
                return CreatedAtAction(nameof(GetReward), new { id = reward.Id }, reward);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reward");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> ApproveReward(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _rewardService.ApproveRewardAsync(id, currentUserId);
                if (!result)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving reward {RewardId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/upload-document")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> UploadRewardDocument(int id, IFormFile file)
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

                var folderName = $"rewards/{id}";
                using var stream = file.OpenReadStream();
                var filePath = await _minIOService.UploadFileAsync(stream, file.FileName, contentType, folderName);

                var currentUserId = GetCurrentUserId();
                var result = await _rewardService.UploadRewardDocumentAsync(id, filePath, currentUserId);
                
                if (!result)
                {
                    return NotFound();
                }

                return Ok(new { filePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading reward document for reward {RewardId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> DeleteReward(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _rewardService.DeleteRewardAsync(id, currentUserId);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reward {RewardId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
