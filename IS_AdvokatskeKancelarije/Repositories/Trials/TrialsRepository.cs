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
            var client = await _context.Clients.FindAsync(caseId.ClientId);
            string emailBody = $@"Dear {client.Name},

We would like to inform you that a trial has been scheduled regarding your case '{caseId.Name}'.

Trial Details:
- Court: {newTrial.Court}
- Judge: {newTrial.Judge}
- Date: {newTrial.CreatedDate:dddd, MMMM dd, yyyy}
- Pre-Trial Preparation: {newTrial.PreTrialPreparation}
- Evidence: {newTrial.Evidence}

Please ensure you are available at the specified time. If you have any questions or concerns,
please do not hesitate to contact us at your earliest convenience.

We kindly ask you to confirm your attendance by replying to this email.

Kind regards,
S & B Law Firm
Legal Department";

            await _emailService.SendEmail(client.Email, "Trial Scheduled - S & B Law Firm", emailBody);
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

            var caseId = await _context.Cases.FindAsync(trial.CaseId);
            var client = await _context.Clients.FindAsync(caseId.ClientId);
            string emailBody = $@"Dear {client.Name},

We would like to inform you that a trial has been updated regarding your case '{caseId.Name}'.

Trial Details:
- Court: {trial.Court}
- Judge: {trial.Judge}
- Date: {trial.CreatedDate:dddd, MMMM dd, yyyy}
- Pre-Trial Preparation: {trial.PreTrialPreparation}
- Evidence: {trial.Evidence}

Please take note of the updated details. If you have any questions or concerns,
please do not hesitate to contact us at your earliest convenience.

We kindly ask you to confirm your attendance by replying to this email.

Kind regards,
S & B Law Firm
Legal Department";

            await _emailService.SendEmail(client.Email, "Trial Scheduled - S & B Law Firm", emailBody);
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
