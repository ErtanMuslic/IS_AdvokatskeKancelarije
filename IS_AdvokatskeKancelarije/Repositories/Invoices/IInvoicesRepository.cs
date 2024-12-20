using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;

namespace IS_AdvokatskeKancelarije.Repositories.Invoices
{
    public interface IInvoicesRepository
    {
        Task<string> AddInvoice(InvoiceDTO invoiceDTO);
        IEnumerable<Invoice> GetInvoices();
        Invoice GetInvoiceById(Guid id);
        Task UpdateInvoice(Invoice invoice);
        void DeleteInvoice(Guid id);

        bool SaveChanges();
    }
}
