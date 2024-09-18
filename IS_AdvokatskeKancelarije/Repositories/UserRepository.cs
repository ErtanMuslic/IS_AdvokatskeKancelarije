using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using IS_AdvokatskeKancelarije.Data;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Models;
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

         
        public async Task<string> LoginAccount(LoginDTO loginDTO)
        {
            if (loginDTO == null) return "Login container is empty";

            var getUser = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (getUser is null)
            {
                return "User not found";
            }

            var checkUserPassword = await _userManager.CheckPasswordAsync(getUser, loginDTO.Password);
            if (getUser is null)
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

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

    }
}
