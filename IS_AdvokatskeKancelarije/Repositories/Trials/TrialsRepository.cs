using AutoMapper;
using IS_AdvokatskeKancelarije.Data;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;
using IS_AdvokatskeKancelarije.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace IS_AdvokatskeKancelarije.Repositories.Trials
{
    public class TrialsRepository : ITrialsRepository
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly EmailService _emailService;


        public TrialsRepository(
            AppDbContext context,
            IMapper mapper,
            EmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;

        }


        public async Task<string> AddTrial(TrialDTO trialDTO)
        {
            var newTrial = _mapper.Map<Trial>(trialDTO);
            _context.Trials.Add(newTrial);

            await _context.SaveChangesAsync();

            var caseId = await _context.Cases.FindAsync(trialDTO.CaseId);
            Console.Write(caseId);
            var client = await _context.Clients.FindAsync(caseId.ClientId);
            Console.WriteLine(client);
            await _emailService.SendEmail(
                client.Email,
                "Trial Scheduled",
                $"Your trial is scheduled on {newTrial.CreatedDate} at {newTrial.Court}"
                );

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
