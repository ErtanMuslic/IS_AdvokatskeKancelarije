using AutoMapper;
using IS_AdvokatskeKancelarije.Data;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;
using Microsoft.EntityFrameworkCore;

namespace IS_AdvokatskeKancelarije.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public DocumentRepository(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<string> AddDocument(DocumentDTO documentDTO)
        {
            //documentDTO.CaseId = CaseId;
            var newDocument = _mapper.Map<Document>(documentDTO);
            _context.Documents.Add(newDocument);

            await _context.SaveChangesAsync();

            return "Document Created";
        }

        public async Task<Document> AddDocumentWithFile(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public void Delete(Guid id)
        {
            var document = _context.Documents.Find(id);
            if (document != null)
            {
                _context.Documents.Remove(document);
            }
        }

        public IEnumerable<Document> GetAll()
        {
            return _context.Documents.ToList();
        }

        public Document GetById(Guid id)
        {
            return _context.Documents.Find(id);
        }

        public async Task Update(Document document)
        {
            _context.Documents.Attach(document);
            _context.Entry(document).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

       
    }
}
