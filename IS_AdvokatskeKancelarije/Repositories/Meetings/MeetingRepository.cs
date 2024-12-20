using AutoMapper;
using IS_AdvokatskeKancelarije.Data;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;
using Microsoft.EntityFrameworkCore;

namespace IS_AdvokatskeKancelarije.Repositories.Meetings
{
    public class MeetingRepository : IMeetingRepository
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public MeetingRepository(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<string> AddMeeting(MeetingDTO meetingDTO)
        {
            var newMeeting = _mapper.Map<Meeting>(meetingDTO);
            _context.Meetings.Add(newMeeting);

            await _context.SaveChangesAsync();

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
