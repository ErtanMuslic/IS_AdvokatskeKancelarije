using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;

namespace IS_AdvokatskeKancelarije.Repositories.Trials
{
    public interface ITrialsRepository
    {
        Task<string> AddTrial(TrialDTO trialDTO);
        IEnumerable<Trial> GetTrials();
        Trial GetTrialById(Guid id);
        Task UpdateTrial(Trial trial);
        void DeleteTrial(Guid id);

        bool SaveChanges();
    }
}
