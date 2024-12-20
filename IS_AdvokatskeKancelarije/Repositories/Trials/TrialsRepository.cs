using AutoMapper;
using IS_AdvokatskeKancelarije.Data;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;
using Microsoft.EntityFrameworkCore;

namespace IS_AdvokatskeKancelarije.Repositories.Trials
{
    public class TrialsRepository : ITrialsRepository
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public TrialsRepository(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public async Task<string> AddTrial(TrialDTO trialDTO)
        {
            var newTrial = _mapper.Map<Trial>(trialDTO);
            _context.Trials.Add(newTrial);

            await _context.SaveChangesAsync();

            return "Trial Created";

        }

        public IEnumerable<Trial> GetTrials()
        {
            return _context.Trials.ToList();
        }

        public Trial GetTrialById(Guid id)
        {
            return _context.Trials.Find(id);
        }

        public async Task UpdateTrial(Trial trial)
        {
            _context.Trials.Attach(trial);
            _context.Entry(trial).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public void DeleteTrial(Guid id)
        {
            var trial = _context.Trials.Find(id);
            if (trial != null)
            {
                _context.Trials.Remove(trial);
            }
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

    }
}
