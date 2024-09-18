using AutoMapper;
using IS_AdvokatskeKancelarije.Data;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IS_AdvokatskeKancelarije.Repositories
{
    public class ClientRepository : IClientRepository
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ClientRepository(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public async Task<string> AddClient(ClientDTO clientDTO)
        {
            var newClient = _mapper.Map<Client>(clientDTO);
            _context.Clients.Add(newClient);

            await _context.SaveChangesAsync();

            return "Client Created";

        }

        public IEnumerable<Client> GetClients()
        {
            return _context.Clients.ToList();
        }

        public Client GetClientById(Guid id)
        {
            return _context.Clients.Find(id);
        }

        public async Task UpdateClient(Client client)
        {
            _context.Clients.Attach(client);
            _context.Entry(client).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public void DeleteClient(Guid id)
        {
            var client = _context.Clients.Find(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
            }
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

    }
}

