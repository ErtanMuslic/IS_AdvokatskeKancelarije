using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;

namespace IS_AdvokatskeKancelarije.Repositories
{
    public interface IClientRepository
    {
        Task<string> AddClient(ClientDTO clientDTO);
        IEnumerable<Client> GetClients();
        Client GetClientById(Guid id);
        Task UpdateClient(Client client);
        void DeleteClient(Guid id);

        bool SaveChanges();
    }
}
