using AutoMapper;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IS_AdvokatskeKancelarije.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaseController : ControllerBase
    {
        private readonly ICaseRepository _caseRepository;
        private readonly IMapper _mapper;

        public CaseController(ICaseRepository caseRepository, IMapper mapper)
        {
            _caseRepository = caseRepository;
            _mapper = mapper;
        }

        // POST: api/Case/AddCase
        [HttpPost("AddCase")]
        [Authorize(Roles = "Admin,Lawler")]
        public async Task<IActionResult> CreateCase([FromBody] CaseDTO CaseDTO)
        {
            if (CaseDTO == null)
                return BadRequest("Case data is empty");

            var result = await _caseRepository.AddCase(CaseDTO);

            if (result == "Case registered already")
                return Conflict(result);

            return Ok(result);
        }

        // GET: api/Case/all
        [HttpGet("all")]
        //[Authorize(Roles = "Admin")]  
        public IActionResult GetAllCases()
        {
            var Cases = _caseRepository.GetCases();
            return Ok(Cases);
        }

        // GET: api/Case/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Lawyer")]
        public IActionResult GetCaseById(Guid id)
        {
            var Case = _caseRepository.GetCaseById(id);
            if (Case == null)
                return NotFound("Case not found");

            return Ok(Case);
        }

        // PUT: api/Case/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCase(Guid id, [FromBody] CaseDTO CaseDTO)
        {
            if (CaseDTO == null)
                return BadRequest("Case data is empty");

            var Case = _caseRepository.GetCaseById(id);
            if (Case == null)
                return NotFound("Case not found");

            _mapper.Map(CaseDTO, Case);

            await _caseRepository.UpdateCase(Case);

            return Ok("Case updated successfully");
        }

        // DELETE: api/Case/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteCase(Guid id)
        {
            var Case = _caseRepository.GetCaseById(id);
            if (Case == null)
                return NotFound("Case not found");

            _caseRepository.DeleteCase(id);
            var result = _caseRepository.SaveChanges();

            if (!result)
                return StatusCode(500, "Error deleting the Case");

            return Ok("Case deleted successfully");
        }
    }
}

