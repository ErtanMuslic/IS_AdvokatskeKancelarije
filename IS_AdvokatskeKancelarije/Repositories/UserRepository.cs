using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using IS_AdvokatskeKancelarije.Data;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace IS_AdvokatskeKancelarije.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public UserRepository(
            AppDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager,
            IConfiguration config,
            IMapper mapper)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _config = config;
            _mapper = mapper;
        }


        public record UserSession(string Email, List<string> Roles);

        public async Task<string> Register(UserDTO userDTO)
        {
            var newUser = _mapper.Map<User>(userDTO);
            var user = await _userManager.FindByEmailAsync(newUser.Email);
            if (user is not null) return "User registered already";

            var createUser = await _userManager.CreateAsync(newUser!, userDTO.Password);
            if (!createUser.Succeeded)
            {
                return $"Error: {string.Join(", ", createUser.Errors.Select(e => e.Description))}";
            }

            var checkAdmin = await _roleManager.FindByNameAsync("Admin");
            if (checkAdmin is null)
            {
                await _roleManager.CreateAsync(new IdentityRole() { Name = "Lawyer" });
                await _roleManager.CreateAsync(new IdentityRole() { Name = "Admin" });
                await _roleManager.CreateAsync(new IdentityRole() { Name = "Paralegal" });


                await _userManager.AddToRoleAsync(newUser, "Lawyer");
                await _userManager.AddToRoleAsync(newUser, "Admin");
                await _userManager.AddToRoleAsync(newUser, "Paralegal");
                return "Account Created";

            }
            else
            {
                await _userManager.AddToRoleAsync(newUser, "Lawyer");
                return "Account Created";
            }
        }

        public async Task<string> UpdateRole(string userId,bool promotion)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return $"User with ID {userId} not found.";

            var currentRoles = await _userManager.GetRolesAsync(user);

           

            var currentRole = currentRoles.FirstOrDefault();
            if (string.IsNullOrEmpty(currentRole))
                return "User has no role assigned.";

            string newRole = currentRole switch
            {
                "Paralegal" when promotion => "Lawyer",
                "Lawyer" when !promotion => "Paralegal",
                _ => null
            };

            if (newRole is null)
                return "Invalid promotion/demotion request or unsupported role transition.";

            var removeRoleResult = await _userManager.RemoveFromRoleAsync(user, currentRole);
            if (!removeRoleResult.Succeeded)
                return $"Failed to remove user from role '{currentRole}': {string.Join(", ", removeRoleResult.Errors.Select(e => e.Description))}";

            var addRoleResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addRoleResult.Succeeded)
                return $"Failed to assign user to role '{newRole}': {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}";

            return $"User role successfully updated to '{newRole}'.";
        }


        public async  Task<List<string>> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);

            return roles.ToList();
        }

        public async Task<string> ChangePasswordByEmail(ChangePasswordDTO changePasswordDTO)
        {
            var user = await _userManager.FindByEmailAsync(changePasswordDTO.Email);
            if (user == null)
            {
                return "User not found.";
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);
            if (!result.Succeeded)
            {
                return $"Error: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return "Password changed successfully.";
        }


        public async Task<string> LoginAccount(LoginDTO loginDTO)
        {
            if (loginDTO == null) return "Login container is empty";

            var getUser = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (getUser is null)
            {
                return "User not found";
            }

            var checkUserPassword = await _userManager.CheckPasswordAsync(getUser, loginDTO.Password);
            if (!checkUserPassword)
            {
                return "Invalid email/Password";
            }

            var getUserRoles = await _userManager.GetRolesAsync(getUser);
            var userSession = new UserSession(getUser.Email, getUserRoles.ToList());
            string token = GenerateToken(userSession);
            return token;

        }

        private string GenerateToken(UserSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email)
            };
            userClaims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var audiences = _config.GetSection("Jwt:Audiences").Get<List<string>>();
            userClaims.AddRange(audiences.Select(audience => new Claim(JwtRegisteredClaimNames.Aud, audience)));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void Delete(string id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
        }

        public IEnumerable<User> GetCaseUsers(Guid caseId)
        {
            return _context.Users
                  .Where(c => c.Cases.Any(u => u.Id == caseId))
                  .ToList();
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users.ToList();
        }

        public User GetById(string id)
        {
            return _context.Users.Find(id);
        }

        public async Task Update(User user)
        {
            _context.Users.Attach(user);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }



        public async Task<bool> AddUserToCase(string userId, Guid caseId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var caseEntity = await _context.Cases
                    .Include(c => c.Users)
                    .FirstOrDefaultAsync(c => c.Id == caseId);


                if (caseEntity.Users == null)
                {
                    caseEntity.Users = new List<User>();
                }
                if (user.Cases == null)
                {
                    user.Cases = new List<Case>();
                }

                if (caseEntity.Users.Count >= 3)
                {
                    throw new InvalidOperationException("This case already has the maximum number of users (3).");
                }
                caseEntity.Users.Add(user);
                if (caseEntity.Status != Enums.Status.Closed)
                {
                    caseEntity.Status = Enums.Status.Active;
                }

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> RemoveUserFromCase(string userId, Guid caseId)
        {
            try
            {
                var caseEntity = await _context.Cases
                    .Include(c => c.Users)
                    .FirstOrDefaultAsync(c => c.Id == caseId);

                if (caseEntity == null)
                    throw new Exception("Case not found.");

                var user = caseEntity.Users
                    .FirstOrDefault(u => u.Id == userId);

                if (user == null)
                    throw new Exception("User not assigned to this case.");

                caseEntity.Users.Remove(user);

                if(caseEntity.Users.Count == 0)
                {
                    caseEntity.Status = Enums.Status.Pending;
                }

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }



        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }
    }
}
