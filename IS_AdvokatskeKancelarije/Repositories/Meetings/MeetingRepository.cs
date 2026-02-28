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

            string emailBody = $@"Dear {client.Name},

We would like to inform you that a meeting has been scheduled regarding your case '{caseId.Name}'.

Meeting Details:
- Date & Time: {newMeeting.Date:dddd, MMMM dd, yyyy} at {newMeeting.Date:hh:mm tt}
- Location: {newMeeting.Location}
- Duration: {newMeeting.Duration} minutes
- Subject: {newMeeting.Subject}

Please ensure you are available at the specified time. If you need to reschedule or have any questions, 
please do not hesitate to contact us at your earliest convenience.

We kindly ask you to confirm your attendance by replying to this email.

Kind regards,
S & B Law Firm
Legal Department";

            await _emailService.SendEmail(client.Email, "Meeting Scheduled - S & B Law Firm", emailBody);


            //await _emailService.SendEmail(
            //    client.Email,
            //    "Meeting Scheduled",
            //    $"Meeting scheduled on {newMeeting.Date}  at  {newMeeting.Location}"
            //    );

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

            var caseId = await _context.Cases.FindAsync(meeting.CaseId);
            var client = await _context.Clients.FindAsync(caseId.ClientId);
            string emailBody = $@"Dear {client.Name},

We would like to inform you that a meeting has been updated  regarding your case '{caseId.Name}'.

Meeting Details:
- Date & Time: {meeting.Date:dddd, MMMM dd, yyyy} at {meeting.Date:hh:mm tt}
- Location: {meeting.Location}
- Duration: {meeting.Duration} minutes
- Subject: {meeting.Subject}

Please take note of the updated details. If you need to reschedule or have any questions,
please do not hesitate to contact us at your earliest convenience.

We kindly ask you to confirm your attendance by replying to this email.

Kind regards,
S & B Law Firm
Legal Department";

            await _emailService.SendEmail(client.Email, "Meeting Scheduled - S & B Law Firm", emailBody);
            await _context.SaveChangesAsync();
        }
        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

    }
}
