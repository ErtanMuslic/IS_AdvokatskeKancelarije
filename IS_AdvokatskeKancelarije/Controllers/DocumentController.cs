using AutoMapper;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Repositories;
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

        public DocumentController(IDocumentRepository documentRepository, IMapper mapper)
        {
            _documentRepository = documentRepository;
            _mapper = mapper;
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

            _documentRepository.Delete(id);
            var result = _documentRepository.SaveChanges();

            if (!result)
                return StatusCode(500, "Error deleting the Document");

            return Ok("Document deleted successfully");
        }
    }
}