using AutoMapper;
using IS_AdvokatskeKancelarije.Data;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;
using Microsoft.EntityFrameworkCore;

namespace IS_AdvokatskeKancelarije.Repositories
{
    public class CaseRepository : ICaseRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CaseRepository(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<string> AddCase(CaseDTO caseDTO)
        {
            var newCase = _mapper.Map<Case>(caseDTO);
            _context.Cases.Add(newCase);

            await _context.SaveChangesAsync();

            return "Case Created";
        }

        public IEnumerable<Case> GetCases()
        {
            return _context.Cases.ToList();
        }

        public Case GetCaseById(Guid id)
        {
            return _context.Cases.Find(id);
        }

        public async Task UpdateCase(Case Case)
        {
            _context.Cases.Attach(Case);
            _context.Entry(Case).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public void DeleteCase(Guid id)
        {
            var casse = _context.Cases.Find(id);
            if(casse != null )
            {
                _context.Cases.Remove(casse);
            }
        }


        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

    }
}
