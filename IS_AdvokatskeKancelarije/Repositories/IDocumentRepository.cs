using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;

namespace IS_AdvokatskeKancelarije.Repositories
{
    public interface IDocumentRepository
    {
        Task<string> AddDocument(DocumentDTO documentDTO);
        Task<Document> AddDocumentWithFile(Document document);
        IEnumerable<Document> GetAll();
        Document GetById(Guid id);
        Task Update(Document document);
        void Delete(Guid id);

        bool SaveChanges();
    }
}
