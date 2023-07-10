using LoLApiNET7.Dto;
using LoLApiNET7.Models;
using LoLApiNET7.Services;
using Microsoft.AspNetCore.Authorization;
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
            //This gets the users, and maps the user to only return its username and role. No other information is returned.
            var users = _userService.GetUsers();
            var usersList = new List<User>();

            foreach (var user in users)
            {
                var usersMap = new User
                {
                    User_Id = user.User_Id,
                    Username = user.Username,
                    Role = user.Role
                };
                usersList.Add(usersMap);
            }

            return Ok(usersList);
        }

        [HttpPost("register")]
        public IActionResult CreateUser(User u)
        {
            // Validations before posting
            if (string.IsNullOrEmpty(u.Username))
                return BadRequest("Username is required");

            if (u.Username.Length > 16)
                return BadRequest("Username's cannot exceed 16 characters");

            if (_userService.UserExists(u.Username))
                return Conflict("This username is already taken");

            if (string.IsNullOrEmpty(u.Email))
                return BadRequest("Email is required");

            if (_userService.EmailExists(u.Email))
                return Conflict("This email is already taken");

            if (!u.Email.Contains('@') || !_userService.ValidateEmail(u.Email)) // If email does not contain @ sign or the validations i.e. .com, .net...
                return BadRequest("Please introduce a valid email address");

            if (string.IsNullOrEmpty(u.Role))
                return BadRequest("Role cannot be empty");

            string role = u.Role.ToLower();

            if (role != "admin" && role != "user")
                return BadRequest("Role should be either ADMIN or USER");

            if (string.IsNullOrEmpty(u.Password))
                return BadRequest("Password is required");

            if (u.Password.Length < 6)
                return BadRequest("Password must be at least 6 characters long");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_userService.CreateUser(u))
            {
                ModelState.AddModelError("", "Something went wrong while registering the user");
                return StatusCode(500, ModelState);
            }

            return Ok($"User {u.Username} has been created");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDto u)
        {
            if (!_userService.UserExists(u.Username)) //Checking if inputted username exists
                return BadRequest($"{u.Username} does not exist");

            if (!BCrypt.Net.BCrypt.Verify(u.Password, _userService.GetPassword(u.Username))) //Comparing the inputted password with the hashed password in the database
                return BadRequest("Wrong password");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userLogin = _userService.GetUser(u.Username);

            var token = _userService.CreateToken(userLogin); //Creating the token

            //return Ok(new { Token = token });
            return Ok(token); //returning the token
        }

        [HttpDelete("id/{userId}")] // Add auth later. Only a logged user can delete himself
        [Authorize(Policy = "UserAllowed")]
        public IActionResult DeleteUser(int userId)
        {
            if (!_userService.UserExistsId(userId))
                return BadRequest("Thi username does not exist or has already been deleted");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userToDelete = _userService.GetUserById(userId);

            if (!_userService.DeleteUser(userToDelete))
            {
                ModelState.AddModelError("", "Something went wrong while deleting the user. Are you the user you are trying to delete?");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}
