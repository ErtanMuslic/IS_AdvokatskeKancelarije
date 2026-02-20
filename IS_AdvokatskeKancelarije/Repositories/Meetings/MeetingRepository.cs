using AutoMapper;
using IS_AdvokatskeKancelarije.Data;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;
using IS_AdvokatskeKancelarije.Services;
using Microsoft.EntityFrameworkCore;

namespace IS_AdvokatskeKancelarije.Repositories.Meetings
{
    public class MeetingRepository : IMeetingRepository
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly EmailService _emailService;

        public MeetingRepository(
            AppDbContext context,
            IMapper mapper,
            EmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
        }
        public async Task<string> AddMeeting(MeetingDTO meetingDTO)
        {
            var newMeeting = _mapper.Map<Meeting>(meetingDTO);
            _context.Meetings.Add(newMeeting);

            await _context.SaveChangesAsync();

            var caseId = await _context.Cases.FindAsync(meetingDTO.CaseId);
            Console.Write(caseId);
            var client = await _context.Clients.FindAsync(caseId.ClientId);
            Console.WriteLine(client);
            await _emailService.SendEmail(
                client.Email,
                "Meeting Scheduled",
                $"Meeting scheduled on {newMeeting.Date}  at  {newMeeting.Location}"
                );

            return "Meeting Created";
        }

        public void DeleteMeeting(Guid id)
        {
            var meeting = _context.Meetings.Find(id);
            if (meeting != null)
            {
                _context.Meetings.Remove(meeting);
            }
        }

        public Meeting GetMeetingById(Guid id)
        {
            return _context.Meetings.Find(id);
        }

        public IEnumerable<Meeting> GetMeetings()
        {
            return _context.Meetings.ToList();
        }


        public async Task UpdateMeeting(Meeting meeting)
        {
            _context.Meetings.Attach(meeting);
            _context.Entry(meeting).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

    }
}
