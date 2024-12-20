using AutoMapper;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Repositories;
using IS_AdvokatskeKancelarije.Repositories.Trials;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IS_AdvokatskeKancelarije.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrialController : ControllerBase
    {
        private readonly ITrialsRepository _trialRepository;
        private readonly IMapper _mapper;

        public TrialController(ITrialsRepository trialRepository, IMapper mapper)
        {
            _trialRepository = trialRepository;
            _mapper = mapper;
        }

        // POST: api/Trial/AddTrial
        [HttpPost("AddTrial")]
        [Authorize(Roles = "Admin,Lawler")]
        public async Task<IActionResult> CreateTrial([FromBody] TrialDTO trialDTO)
        {
            if (trialDTO == null)
                return BadRequest("Trial data is empty");

            var result = await _trialRepository.AddTrial(trialDTO);

            if (result == "Trial registered already")
                return Conflict(result);

            return Ok(result);
        }

        // GET: api/Trial/all
        [HttpGet("all")]
        //[Authorize(Roles = "Admin")]  
        public IActionResult GetAllTrials()
        {
            var trials = _trialRepository.GetTrials();
            return Ok(trials);
        }

        // GET: api/Trial/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Lawyer")]
        public IActionResult GetTrialById(Guid id)
        {
            var trial = _trialRepository.GetTrialById(id);
            if (trial == null)
                return NotFound("Trial not found");

            return Ok(trial);
        }

        // PUT: api/Trial/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTrial(Guid id, [FromBody] TrialDTO trialDTO)
        {
            if (trialDTO == null)
                return BadRequest("Trial data is empty");

            var trial = _trialRepository.GetTrialById(id);
            if (trial == null)
                return NotFound("Trial not found");

            _mapper.Map(trialDTO, trial);

            await _trialRepository.UpdateTrial(trial);

            return Ok("Trial updated successfully");
        }

        // DELETE: api/Trial/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteTrial(Guid id)
        {
            var trial = _trialRepository.GetTrialById(id);
            if (trial == null)
                return NotFound("Trial not found");

            _trialRepository.DeleteTrial(id);
            var result = _trialRepository.SaveChanges();

            if (!result)
                return StatusCode(500, "Error deleting the Trial");

            return Ok("Trial deleted successfully");
        }
    }
}
