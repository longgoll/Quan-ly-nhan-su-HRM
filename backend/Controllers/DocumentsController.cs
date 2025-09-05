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
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(IDocumentService documentService, ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet]
        public async Task<ActionResult<List<DocumentDto>>> GetAllDocuments()
        {
            try
            {
                var documents = await _documentService.GetAllDocumentsAsync();
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all documents");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetDocument(int id)
        {
            try
            {
                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document == null)
                {
                    return NotFound();
                }
                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document {DocumentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<List<DocumentDto>>> GetDocumentsByEmployee(int employeeId)
        {
            try
            {
                var documents = await _documentService.GetDocumentsByEmployeeIdAsync(employeeId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents for employee {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("type/{type}")]
        public async Task<ActionResult<List<DocumentDto>>> GetDocumentsByType(DocumentType type)
        {
            try
            {
                var documents = await _documentService.GetDocumentsByTypeAsync(type);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents by type {DocumentType}", type);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("upload")]
        [Authorize(Roles = "Employee,Manager,HRManager,Admin")]
        public async Task<ActionResult<DocumentDto>> UploadDocument([FromForm] UploadDocumentDto uploadDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var document = await _documentService.UploadDocumentAsync(uploadDto, currentUserId);
                return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            try
            {
                var fileResponse = await _documentService.DownloadDocumentAsync(id);
                if (fileResponse == null)
                {
                    return NotFound();
                }

                return File(fileResponse.FileStream, fileResponse.ContentType, fileResponse.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document {DocumentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _documentService.DeleteDocumentAsync(id, currentUserId);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
