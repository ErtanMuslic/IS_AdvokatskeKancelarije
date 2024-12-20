using AutoMapper;
using IS_AdvokatskeKancelarije.Data;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;
using Microsoft.EntityFrameworkCore;

namespace IS_AdvokatskeKancelarije.Repositories.Invoices
{
    public class InvoicesRepository : IInvoicesRepository
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public InvoicesRepository(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public async Task<string> AddInvoice(InvoiceDTO invoiceDTO)
        {
            var newInvoice = _mapper.Map<Invoice>(invoiceDTO);
            _context.Invoices.Add(newInvoice);

            await _context.SaveChangesAsync();

            return "Invoice Created";

        }

        public IEnumerable<Invoice> GetInvoices()
        {
            return _context.Invoices.ToList();
        }

        public Invoice GetInvoiceById(Guid id)
        {
            return _context.Invoices.Find(id);
        }

        public async Task UpdateInvoice(Invoice invoice)
        {
            _context.Invoices.Attach(invoice);
            _context.Entry(invoice).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public void DeleteInvoice(Guid id)
        {
            var invoice = _context.Invoices.Find(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
            }
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

    }
}


