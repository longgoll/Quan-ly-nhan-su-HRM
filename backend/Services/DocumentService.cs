using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using AutoMapper;

namespace backend.Services
{
    public interface IDocumentService
    {
        Task<List<DocumentDto>> GetAllDocumentsAsync();
        Task<DocumentDto?> GetDocumentByIdAsync(int id);
        Task<List<DocumentDto>> GetDocumentsByEmployeeIdAsync(int employeeId);
        Task<DocumentDto> UploadDocumentAsync(UploadDocumentDto uploadDto, int uploadedById);
        Task<FileResponseDto?> DownloadDocumentAsync(int id);
        Task<bool> DeleteDocumentAsync(int id, int deletedById);
        Task<List<DocumentDto>> GetDocumentsByTypeAsync(DocumentType type);
    }

    public class DocumentService : IDocumentService
    {
        private readonly HrmDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMinIOService _minIOService;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(
            HrmDbContext context, 
            IMapper mapper, 
            IMinIOService minIOService,
            ILogger<DocumentService> logger)
        {
            _context = context;
            _mapper = mapper;
            _minIOService = minIOService;
            _logger = logger;
        }

        public async Task<List<DocumentDto>> GetAllDocumentsAsync()
        {
            try
            {
                var documents = await _context.EmployeeDocuments
                    .Include(d => d.Employee)
                    .Include(d => d.UploadedBy)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<DocumentDto>>(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all documents");
                throw new InvalidOperationException("Failed to retrieve documents", ex);
            }
        }

        public async Task<DocumentDto?> GetDocumentByIdAsync(int id)
        {
            try
            {
                var document = await _context.EmployeeDocuments
                    .Include(d => d.Employee)
                    .Include(d => d.UploadedBy)
                    .FirstOrDefaultAsync(d => d.Id == id);

                return document != null ? _mapper.Map<DocumentDto>(document) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document by ID: {DocumentId}", id);
                throw new InvalidOperationException($"Failed to retrieve document with ID {id}", ex);
            }
        }

        public async Task<List<DocumentDto>> GetDocumentsByEmployeeIdAsync(int employeeId)
        {
            try
            {
                var documents = await _context.EmployeeDocuments
                    .Include(d => d.Employee)
                    .Include(d => d.UploadedBy)
                    .Where(d => d.EmployeeId == employeeId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<DocumentDto>>(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents for employee: {EmployeeId}", employeeId);
                throw new InvalidOperationException($"Failed to retrieve documents for employee {employeeId}", ex);
            }
        }

        public async Task<DocumentDto> UploadDocumentAsync(UploadDocumentDto uploadDto, int uploadedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if employee exists
                var employee = await _context.Employees.FindAsync(uploadDto.EmployeeId);
                if (employee == null)
                {
                    throw new ArgumentException("Employee not found");
                }

                // Validate file
                if (uploadDto.File == null || uploadDto.File.Length == 0)
                {
                    throw new ArgumentException("File is required");
                }

                var allowedTypes = new[] { "pdf", "doc", "docx", "jpg", "jpeg", "png", "gif" };
                var fileExtension = Path.GetExtension(uploadDto.File.FileName).ToLowerInvariant().TrimStart('.');
                
                if (!allowedTypes.Contains(fileExtension))
                {
                    throw new ArgumentException($"File type .{fileExtension} is not allowed");
                }

                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (uploadDto.File.Length > maxFileSize)
                {
                    throw new ArgumentException("File size cannot exceed 10MB");
                }

                // Upload file to MinIO
                var contentType = GetContentType(fileExtension);
                var folderName = $"employees/{uploadDto.EmployeeId}/documents";
                
                using var stream = uploadDto.File.OpenReadStream();
                var filePath = await _minIOService.UploadFileAsync(stream, uploadDto.File.FileName, contentType, folderName);

                // Save document info to database
                var document = new EmployeeDocument
                {
                    EmployeeId = uploadDto.EmployeeId,
                    Title = uploadDto.Title,
                    Type = uploadDto.Type,
                    FilePath = filePath,
                    FileName = uploadDto.File.FileName,
                    FileType = fileExtension,
                    FileSize = uploadDto.File.Length,
                    Description = uploadDto.Description,
                    UploadedById = uploadedById,
                    CreatedAt = DateTime.UtcNow
                };

                _context.EmployeeDocuments.Add(document);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Document uploaded successfully: {DocumentId} by user {UserId}", document.Id, uploadedById);
                return await GetDocumentByIdAsync(document.Id) ?? throw new InvalidOperationException("Failed to retrieve uploaded document");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error uploading document for employee {EmployeeId}", uploadDto.EmployeeId);
                throw;
            }
        }

        public async Task<FileResponseDto?> DownloadDocumentAsync(int id)
        {
            try
            {
                var document = await _context.EmployeeDocuments.FindAsync(id);
                if (document == null)
                {
                    return null;
                }

                var fileStream = await _minIOService.DownloadFileAsync(document.FilePath);
                var contentType = GetContentType(document.FileType ?? "");

                return new FileResponseDto
                {
                    FileName = document.FileName,
                    ContentType = contentType,
                    FileStream = fileStream
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document: {DocumentId}", id);
                throw new InvalidOperationException($"Failed to download document with ID {id}", ex);
            }
        }

        public async Task<bool> DeleteDocumentAsync(int id, int deletedById)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var document = await _context.EmployeeDocuments.FindAsync(id);
                if (document == null)
                {
                    return false;
                }

                // Delete file from MinIO
                await _minIOService.DeleteFileAsync(document.FilePath);

                // Delete record from database
                _context.EmployeeDocuments.Remove(document);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Document deleted successfully: {DocumentId} by user {UserId}", id, deletedById);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting document {DocumentId}", id);
                return false;
            }
        }

        public async Task<List<DocumentDto>> GetDocumentsByTypeAsync(DocumentType type)
        {
            try
            {
                var documents = await _context.EmployeeDocuments
                    .Include(d => d.Employee)
                    .Include(d => d.UploadedBy)
                    .Where(d => d.Type == type)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<DocumentDto>>(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents by type: {DocumentType}", type);
                throw new InvalidOperationException($"Failed to retrieve documents of type {type}", ex);
            }
        }

        private static string GetContentType(string fileExtension)
        {
            return fileExtension.ToLowerInvariant() switch
            {
                "pdf" => "application/pdf",
                "doc" => "application/msword",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}
