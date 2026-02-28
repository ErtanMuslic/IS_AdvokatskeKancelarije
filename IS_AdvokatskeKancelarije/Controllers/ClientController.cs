using AutoMapper;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;
using IS_AdvokatskeKancelarije.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IS_AdvokatskeKancelarije.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;

        public ClientController(IClientRepository clientRepository, IMapper mapper)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
        }

        // POST: api/Client/AddClient
        [HttpPost("AddClient")]
        [Authorize(Roles = "Admin,Lawyer")]
        public async Task<IActionResult> CreateClient([FromBody] ClientDTO clientDTO)
        {
            if (clientDTO == null)
                return BadRequest("Client data is empty");

            var result = await _clientRepository.AddClient(clientDTO);

            if (result == "Client registered already")
                return Conflict(result);

            return Ok(result);
        }

        // GET: api/Client/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]  
        public IActionResult GetAllClients()
        {
            var clients = _clientRepository.GetClients();
            return Ok(clients);
        }

        // GET: api/Client/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Lawyer,Paralegal")]
        public IActionResult GetClientById(Guid id)
        {
            var client = _clientRepository.GetClientById(id);
            if (client == null)
                return NotFound("Client not found");

            return Ok(client);
        }

        // PUT: api/Client/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Lawyer")]
        public async Task<IActionResult> UpdateClient(Guid id, [FromBody] ClientDTO clientDTO)
        {
            if (clientDTO == null)
                return BadRequest("Client data is empty");

            var client = _clientRepository.GetClientById(id);
            if (client == null)
                return NotFound("Client not found");

            _mapper.Map(clientDTO, client);

            await _clientRepository.UpdateClient(client);

            return Ok("Client updated successfully");
        }

        // DELETE: api/Client/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteClient(Guid id)
        {
            var client = _clientRepository.GetClientById(id);
            if (client == null)
                return NotFound("Client not found");

            _clientRepository.DeleteClient(id);
            var result = _clientRepository.SaveChanges();

            if (!result)
                return StatusCode(500, "Error deleting the Client");

            return Ok("Client deleted successfully");
        }
    }
}
