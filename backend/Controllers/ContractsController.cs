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
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IMinIOService _minIOService;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(
            IContractService contractService, 
            IMinIOService minIOService,
            ILogger<ContractsController> logger)
        {
            _contractService = contractService;
            _minIOService = minIOService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet]
        public async Task<ActionResult<List<ContractDto>>> GetAllContracts()
        {
            try
            {
                var contracts = await _contractService.GetAllContractsAsync();
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all contracts");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContractDto>> GetContract(int id)
        {
            try
            {
                var contract = await _contractService.GetContractByIdAsync(id);
                if (contract == null)
                {
                    return NotFound();
                }
                return Ok(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contract {ContractId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<List<ContractDto>>> GetContractsByEmployee(int employeeId)
        {
            try
            {
                var contracts = await _contractService.GetContractsByEmployeeIdAsync(employeeId);
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contracts for employee {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<ActionResult<ContractDto>> CreateContract(CreateContractDto createDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var contract = await _contractService.CreateContractAsync(createDto, currentUserId);
                return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, contract);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<ActionResult<ContractDto>> UpdateContract(int id, UpdateContractDto updateDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var contract = await _contractService.UpdateContractAsync(id, updateDto, currentUserId);
                return Ok(contract);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract {ContractId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/terminate")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> TerminateContract(int id, [FromBody] TerminateContractRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _contractService.TerminateContractAsync(id, request.Reason, currentUserId);
                if (!result)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error terminating contract {ContractId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/upload-document")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> UploadContractDocument(int id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("File is required");
                }

                var allowedTypes = new[] { "pdf", "doc", "docx" };
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
                    _ => "application/octet-stream"
                };

                var folderName = $"contracts/{id}";
                using var stream = file.OpenReadStream();
                var filePath = await _minIOService.UploadFileAsync(stream, file.FileName, contentType, folderName);

                var currentUserId = GetCurrentUserId();
                var result = await _contractService.UploadContractDocumentAsync(id, filePath, currentUserId);
                
                if (!result)
                {
                    return NotFound();
                }

                return Ok(new { filePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading contract document for contract {ContractId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/download-document")]
        public async Task<IActionResult> DownloadContractDocument(int id)
        {
            try
            {
                var contract = await _contractService.GetContractByIdAsync(id);
                if (contract == null || string.IsNullOrEmpty(contract.DocumentPath))
                {
                    return NotFound();
                }

                var fileStream = await _minIOService.DownloadFileAsync(contract.DocumentPath);
                var fileName = Path.GetFileName(contract.DocumentPath);
                
                return File(fileStream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading contract document for contract {ContractId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class TerminateContractRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
