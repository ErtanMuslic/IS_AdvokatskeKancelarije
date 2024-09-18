using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;

namespace IS_AdvokatskeKancelarije.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User GetById(string id);
        Task<string> Register(UserDTO user);
        Task Update(User user);
        void Delete(string id);

        Task<string> LoginAccount(LoginDTO login);
        bool SaveChanges();
    }
}
