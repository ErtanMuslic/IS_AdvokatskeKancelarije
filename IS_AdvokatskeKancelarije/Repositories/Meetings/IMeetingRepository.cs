using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;

namespace IS_AdvokatskeKancelarije.Repositories.Meetings
{
    public interface IMeetingRepository
    {
        Task<string> AddMeeting(MeetingDTO meetingDTO);
        IEnumerable<Meeting> GetMeetings();
        Meeting GetMeetingById(Guid id);
        Task UpdateMeeting(Meeting meeting);
        void DeleteMeeting(Guid id);

        bool SaveChanges();
    }
}
