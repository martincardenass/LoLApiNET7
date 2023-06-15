using LoLApiNET7.Dto;
using LoLApiNET7.Models;
using LoLApiNET7.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoLApiNET7.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet("users")] //Get the users
        [ProducesResponseType(200, Type = typeof(IEnumerable<User>))]
        public IActionResult GetUsers()
        {
            var users = _userService.GetUsers();
            return Ok(users);
        }

        [HttpPost("register")]
        public IActionResult CreateUser(User u)
        {
            //Validations before posting
            if (string.IsNullOrEmpty(u.Username) || string.IsNullOrEmpty(u.Email) || string.IsNullOrEmpty(u.Role))
                return BadRequest("All fields are required");

            if (u.Username.Length > 16)
                return BadRequest("Username's cannot exceed 16 characters");

            string role = u.Role.ToLower();

            if (role != "admin" && role != "user")
                return BadRequest("Role should be either ADMIN or USER");

            if (_userService.UserExists(u.Username))
                return Conflict("This username is already taken");

            if (_userService.EmailExists(u.Email))
                return Conflict("This email is already used");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_userService.CreateUser(u))
            {
                ModelState.AddModelError("", "Something went wrong while registering the user");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDto u)
        {
            if (!_userService.UserExists(u.Username)) //Checking if inputted username exists
                return BadRequest("This username does not exist");

            if (!BCrypt.Net.BCrypt.Verify(u.Password, _userService.GetPassword(u.Username))) //Comparing the inputted password with the hashed password in the database
                return BadRequest("Wrong password!");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userLogin = _userService.GetUser(u.Username);

            var token = _userService.CreateToken(userLogin); //Creating the token

            return Ok(token); //returning the token
        }
    }
}
