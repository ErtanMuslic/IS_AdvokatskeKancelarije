using AutoMapper;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;
using IS_AdvokatskeKancelarije.Repositories;
using IS_AdvokatskeKancelarije.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IS_AdvokatskeKancelarije.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IMapper _mapper;
        private readonly FileStorageService _fileStorage;
        private readonly IndexingQueue _indexingQueue;

        public DocumentController(
            IDocumentRepository documentRepository,
            IMapper mapper,
            FileStorageService fileStorage,
            IndexingQueue indexingQueue)
        {
            _documentRepository = documentRepository;
            _mapper = mapper;
            _fileStorage = fileStorage;
            _indexingQueue = indexingQueue;
        }

        // POST: api/Document/Upload  (multipart/form-data)
        [HttpPost("Upload")]
        [Authorize(Roles = "Paralegal,Lawyer,Admin")]
        [RequestSizeLimit(FileStorageService.MaxFileSizeBytes + 1024 * 1024)]
        public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadDTO dto)
        {
            if (dto.CaseId == Guid.Empty)
                return BadRequest("Case is required");

            var error = await _fileStorage.ValidateAsync(dto.File);
            if (error != null)
                return BadRequest(error);

            var storedName = await _fileStorage.SaveAsync(dto.File);

            var document = new Document
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Author = dto.Author,
                Description = dto.Description,
                Type = dto.Type,
                CaseId = dto.CaseId,
                CreatedDate = DateTime.UtcNow,
                OriginalFileName = Path.GetFileName(dto.File.FileName),
                ContentType = FileStorageService.GetContentType(dto.File.FileName),
                FileSize = dto.File.Length,
                StoredFileName = storedName,
                IndexStatus = "Pending"
            };

            try
            {
                await _documentRepository.AddDocumentWithFile(document);
            }
            catch
            {
                _fileStorage.Delete(storedName);
                throw;
            }

            _indexingQueue.Enqueue(document.Id);

            return Ok(document);
        }

        // POST: api/Document/{id}/reindex
        [HttpPost("{id}/reindex")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]
        public async Task<IActionResult> ReindexDocument(Guid id)
        {
            var document = _documentRepository.GetById(id);
            if (document == null)
                return NotFound("Document not found");

            if (string.IsNullOrEmpty(document.StoredFileName))
                return BadRequest("This document has no uploaded file");

            document.IndexStatus = "Pending";
            document.IndexError = null;
            await _documentRepository.Update(document);

            _indexingQueue.Enqueue(document.Id);

            return Ok(document);
        }

        // GET: api/Document/{id}/download
        [HttpGet("{id}/download")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]
        public IActionResult DownloadDocument(Guid id)
        {
            var document = _documentRepository.GetById(id);
            if (document == null)
                return NotFound("Document not found");

            if (string.IsNullOrEmpty(document.StoredFileName))
                return NotFound("This document has no uploaded file");

            var stream = _fileStorage.Open(document.StoredFileName);
            if (stream == null)
                return NotFound("File not found on server");

            return File(
                stream,
                document.ContentType ?? "application/octet-stream",
                document.OriginalFileName ?? document.StoredFileName);
        }

        // POST: api/Document/Add
        [HttpPost("Add")]
        [Authorize(Roles = "Paralegal,Lawyer,Admin")]
        public async Task<IActionResult> CreateDocument([FromBody] DocumentDTO documentDTO)
        {
            if (documentDTO == null)
                return BadRequest("Document data is empty");

            var result = await _documentRepository.AddDocument(documentDTO);

            return Ok(result);
        }

        // GET: api/Document/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]
        public IActionResult GetAllDocuments()
        {
            var Documents = _documentRepository.GetAll();
            return Ok(Documents);
        }

        // GET: api/Document/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]
        public IActionResult GetDocumentById(Guid id)
        {
            var document = _documentRepository.GetById(id);
            if (document == null)
                return NotFound("Document not found");

            return Ok(document);
        }

        // PUT: api/Document/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]
        public async Task<IActionResult> UpdateDocument(Guid id, [FromBody] DocumentDTO documentDTO)
        {
            if (documentDTO == null)
                return BadRequest("Document data is empty");

            var document = _documentRepository.GetById(id);
            if (document == null)
                return NotFound("Document not found");

            _mapper.Map(documentDTO, document);

            await _documentRepository.Update(document);

            return Ok("Document updated successfully");
        }

        // DELETE: api/Document/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteDocument(Guid id)
        {
            var document = _documentRepository.GetById(id);
            if (document == null)
                return NotFound("Document not found");

            var storedName = document.StoredFileName;

            _documentRepository.Delete(id);
            var result = _documentRepository.SaveChanges();

            if (!result)
                return StatusCode(500, "Error deleting the Document");

            _fileStorage.Delete(storedName);

            return Ok("Document deleted successfully");
        }
    }
}