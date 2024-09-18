using AutoMapper;
using IS_AdvokatskeKancelarije.DTOs;
using IS_AdvokatskeKancelarije.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IS_AdvokatskeKancelarije.Controllers
{
        [ApiController]
        [Route("api/[controller]")]
        public class UserController : ControllerBase
        {
            private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository,IMapper mapper)
            {
                _userRepository = userRepository;
                _mapper = mapper;
            }

            // POST: api/User/register
            [HttpPost("register")]
            public async Task<IActionResult> Register([FromBody] UserDTO userDto)
            {
                if (userDto == null)
                    return BadRequest("User data is empty");

                var result = await _userRepository.Register(userDto);

                if (result == "User registered already")
                    return Conflict(result);

                return Ok(result);
            }

            // POST: api/User/login
            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
            {
                if (loginDto == null)
                    return BadRequest("Login data is empty");

                var token = await _userRepository.LoginAccount(loginDto);

                if (token == "User not found" || token == "Invalid email/Password")
                    return Unauthorized(token);

            //return Ok(new { Token = token });
            return Ok(token);
        }

            // GET: api/User/all
            [HttpGet("all")]
            //[Authorize(Roles = "Admin")]  
            public IActionResult GetAllUsers()
            {
                var users = _userRepository.GetAll();
                return Ok(users);
            }

            // GET: api/User/{id}
            [HttpGet("{id}")]
            [Authorize(Roles = "Admin, Lawyer")]  
            public IActionResult GetUserById(string id)
            {
                var user = _userRepository.GetById(id);
                if (user == null)
                    return NotFound("User not found");

                return Ok(user);
            }

            // PUT: api/User/{id}
            [HttpPut("{id}")]
            [Authorize(Roles = "Admin")]  
            public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDTO userDto)
            {
                if (userDto == null)
                    return BadRequest("User data is empty");

                var user = _userRepository.GetById(id);
                if (user == null)
                    return NotFound("User not found");

                _mapper.Map(userDto, user);

                await _userRepository.Update(user);

                return Ok("User updated successfully");
            }

            // DELETE: api/User/{id}
            [HttpDelete("{id}")]
            [Authorize(Roles = "Admin")]  
            public IActionResult DeleteUser(string id)
            {
                var user = _userRepository.GetById(id);
                if (user == null)
                    return NotFound("User not found");

                _userRepository.Delete(id);
                var result = _userRepository.SaveChanges();

                if (!result)
                    return StatusCode(500, "Error deleting the user");

                return Ok("User deleted successfully");
            }
        }
    }
