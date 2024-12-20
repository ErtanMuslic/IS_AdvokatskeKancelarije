using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;

namespace IS_AdvokatskeKancelarije.Repositories
{
    public interface ICaseRepository
    {
        Task<string> AddCase(CaseDTO caseDTO);
        IEnumerable<Case> GetCases();
        Case GetCaseById(Guid id);
        Task UpdateCase(Case Case);
        void DeleteCase(Guid id);

        bool SaveChanges();
    }
}