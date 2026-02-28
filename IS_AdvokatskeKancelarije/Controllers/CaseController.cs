using AutoMapper;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;
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
        [Authorize(Roles = "Admin,Lawyer")]
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
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]  
        public IActionResult GetAllCases()
        {
            var Cases = _caseRepository.GetCases();
            return Ok(Cases);
        }

        // GET: api/Case/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]
        public IActionResult GetCaseById(Guid id)
        {
            var Case = _caseRepository.GetCaseById(id);
            if (Case == null)
                return NotFound("Case not found");

            return Ok(Case);
        }

        //GET: api/Case/{clientId}
        [HttpGet("Client{clientId}")]
        [Authorize(Roles = "Admin,Lawyer")]
        public IActionResult GetClientCases(Guid clientId)
        {
            var Cases = _caseRepository.GetClientCases(clientId);
            if (Cases == null)
                return NotFound("Client has no cases");

            return Ok(Cases);
        }

        //GET: api/Case/{userId}
        [HttpGet("User{userId}")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]
        public IActionResult GetUserCases(string userId)
        {
            var Cases = _caseRepository.GetUserCases(userId);
            if (Cases == null)
                return NotFound("User has no cases");

            return Ok(Cases);
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


        // PUT: api/Case/{id}
        [HttpPut("Resolve/{id}")]
        [Authorize(Roles = "Admin,Lawyer")]
        public async Task<IActionResult> ResolveCase(Guid id)
        {
            var Case = _caseRepository.GetCaseById(id);
            if (Case == null)
                return NotFound("Case not found");

            Case.Status = Enums.Status.Closed;
            await _caseRepository.UpdateCase(Case);

            return Ok(new { message = "Case updated successfully" });
        }

        // PUT: api/Case/{id}
        [HttpPut("isActive/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> isActive(Guid id)
        {
            var casse = await _caseRepository.GetCaseByIdWithUsers(id); 
            if (casse == null)
                return NotFound("Case not found");

            if (casse.Users != null && casse.Users.Count > 0)
                casse.Status = Enums.Status.Active;

            await _caseRepository.UpdateCase(casse);
            return Ok(new { message = "Case updated successfully" });
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

