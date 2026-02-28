using AutoMapper;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Repositories;
using IS_AdvokatskeKancelarije.Repositories.Meetings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IS_AdvokatskeKancelarije.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingController : ControllerBase
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly IMapper _mapper;

        public MeetingController(IMeetingRepository meetingRepository, IMapper mapper)
        {
            _meetingRepository = meetingRepository;
            _mapper = mapper;
        }

        // POST: api/Meeting/AddMeeting
        [HttpPost("AddMeeting")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]
        public async Task<IActionResult> CreateMeeting([FromBody] MeetingDTO meetingDTO)
        {
            if (meetingDTO == null)
                return BadRequest("Meeting data is empty");

            var result = await _meetingRepository.AddMeeting(meetingDTO);

            if (result == "Meeting registered already")
                return Conflict(result);

            return Ok(result);
        }

        // GET: api/Meeting/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]  
        public IActionResult GetAllMeetings()
        {
            var meetings = _meetingRepository.GetMeetings();
            return Ok(meetings);
        }

        // GET: api/Meeting/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]
        public IActionResult GetMeetingById(Guid id)
        {
            var meeting = _meetingRepository.GetMeetingById(id);
            if (meeting == null)
                return NotFound("Meeting not found");

            return Ok(meeting);
        }

        // PUT: api/Meeting/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]
        public async Task<IActionResult> UpdateMeeting(Guid id, [FromBody] MeetingDTO meetingDTO)
        {
            if (meetingDTO == null)
                return BadRequest("Meeting data is empty");

            var meeting = _meetingRepository.GetMeetingById(id);
            if (meeting == null)
                return NotFound("Meeting not found");

            _mapper.Map(meetingDTO, meeting);

            await _meetingRepository.UpdateMeeting(meeting);

            return Ok("Meeting updated successfully");
        }

        // DELETE: api/Meeting/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteMeeting(Guid id)
        {
            var meeting = _meetingRepository.GetMeetingById(id);
            if (meeting == null)
                return NotFound("Meeting not found");

            _meetingRepository.DeleteMeeting(id);
            var result = _meetingRepository.SaveChanges();

            if (!result)
                return StatusCode(500, "Error deleting the Meeting");

            return Ok("Meeting deleted successfully");
        }
    }
}
