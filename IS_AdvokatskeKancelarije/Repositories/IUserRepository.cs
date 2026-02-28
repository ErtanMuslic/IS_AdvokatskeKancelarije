using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;

namespace IS_AdvokatskeKancelarije.Repositories
{
    public interface IUserRepository
    {

        IEnumerable<User> GetAll();
        Task<bool> AddUserToCase(string userId,Guid caseId);

        Task<bool> RemoveUserFromCase(string userId, Guid caseId);
        User GetById(string id);
        Task<string> Register(UserDTO user);
        Task Update(User user);
        void Delete(string id);
        Task<string> LoginAccount(LoginDTO login);
        public IEnumerable<User> GetCaseUsers(Guid caseId);
        Task<string> ChangePasswordByEmail(ChangePasswordDTO changePasswordDTO);
        Task<List<string>> GetUserRoles(string userId);

        Task<string> UpdateRole(string userId, bool promotion);
        bool SaveChanges();
    }
}
