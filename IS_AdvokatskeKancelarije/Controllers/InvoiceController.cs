using AutoMapper;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Repositories;
using IS_AdvokatskeKancelarije.Repositories.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IS_AdvokatskeKancelarije.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoicesRepository _invoiceRepository;
        private readonly IMapper _mapper;

        public InvoiceController(IInvoicesRepository invoiceRepository, IMapper mapper)
        {
            _invoiceRepository = invoiceRepository;
            _mapper = mapper;
        }

        // POST: api/Invoice/AddInvoice
        [HttpPost("AddInvoice")]
        [Authorize(Roles = "Admin,Lawyer")]
        public async Task<IActionResult> CreateInvoice([FromBody] InvoiceDTO invoiceDTO)
        {
            if (invoiceDTO == null)
                return BadRequest("Invoice data is empty");

            var result = await _invoiceRepository.AddInvoice(invoiceDTO);

            if (result == "Invoice registered already")
                return Conflict(result);

            return Ok(result);
        }

        // GET: api/Invoice/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]  
        public IActionResult GetAllInvoices()
        {
            var invoices = _invoiceRepository.GetInvoices();
            return Ok(invoices);
        }

        // GET: api/Invoice/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]
        public IActionResult GetInvoiceById(Guid id)
        {
            var invoice = _invoiceRepository.GetInvoiceById(id);
            if (invoice == null)
                return NotFound("Invoice not found");

            return Ok(invoice);
        }

        // PUT: api/Invoice/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Lawyer")]
        public async Task<IActionResult> UpdateInvoice(Guid id, [FromBody] InvoiceDTO invoiceDTO)
        {
            if (invoiceDTO == null)
                return BadRequest("Invoice data is empty");

            var invoice = _invoiceRepository.GetInvoiceById(id);
            if (invoice == null)
                return NotFound("Invoice not found");

            _mapper.Map(invoiceDTO, invoice);

            await _invoiceRepository.UpdateInvoice(invoice);

            return Ok("Invoice updated successfully");
        }

        // DELETE: api/Invoice/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteInvoice(Guid id)
        {
            var invoice = _invoiceRepository.GetInvoiceById(id);
            if (invoice == null)
                return NotFound("Invoice not found");

            _invoiceRepository.DeleteInvoice(id);
            var result = _invoiceRepository.SaveChanges();

            if (!result)
                return StatusCode(500, "Error deleting the Invoice");

            return Ok("Invoice deleted successfully");
        }
    }
}
