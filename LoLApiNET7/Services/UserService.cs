using LoLApiNET7.Dto;
using LoLApiNET7.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoLApiNET7.Services
{
    public interface IUserService
    {
        ICollection<User> GetUsers();
        User GetUser(string username);
        User GetUserById(int id);
        string CreateToken(User user);
        int DecodeToken(string token); // This decodes the tokens user ID
        string DecodeTokenRole(string token); // This decodes the tokens role
        //string DecodeToken(JwtSecurityToken securityToken);
        string GetPassword(string username);
        bool ValidateEmail(string email);
        User Authenticate(UserDto user);
        bool UserExists(string username);
        bool UserExistsId(int id); //check if user exists by its Id. Created to use in the reviews.
        bool EmailExists(string email);
        bool ComparedUserIds(int userId); // Compare the UserId From the token to the UserId thats trying to be modified.
        bool IsUserAdmin();
        string GetToken(); // Get the token from the HTTP Auth Headers
        bool CreateUser(User user);
        bool DeleteUser(User user);
        bool Save();
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _accesor; // HttpContext its to get access to the http headers.

        public UserService(AppDbContext context, IConfiguration config, IHttpContextAccessor accesor) 
        {
            _context = context;
            _config = config;
            _accesor = accesor;
        }

        public string CreateToken(User u)
        { //Generate the token 
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, u.User_Id.ToString()), //Convert the userId to string to be able to create a Claim
                new Claim(ClaimTypes.Name, u.Username),
                new Claim(ClaimTypes.Email, u.Email),
                new Claim(ClaimTypes.Role, u.Role)
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(7), // Expires in 7 days = 1 week
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool CreateUser(User user)
        {
            string passwordHashed = BCrypt.Net.BCrypt.HashPassword(user.Password); //Encrypt the user's password

            var userInsert = new User()
            {
                Username = user.Username.ToLower(),
                Created = DateTime.Now,
                Email = user.Email.ToLower(),
                Role = user.Role.ToLower(),
                Password = passwordHashed
            };

            _context.Add(userInsert);
            return Save();

        }

        public bool EmailExists(string email)
        {
            return _context.Users.Any(e => e.Email == email);
        }

        public User GetUser(string username)
        {
            return _context.Users.Where(u => u.Username == username).FirstOrDefault();
        }

        public ICollection<User> GetUsers()
        {
            return _context.Users.OrderBy(u => u.User_Id).ToList();
        }

        public User Authenticate(UserDto user)
        {
            var userLogin = _context.Users.FirstOrDefault(u => u.Username.ToLower() == user.Username.ToLower() && u.Password == user.Password);

            if (userLogin == null)
            {
                return null;
            }

            return userLogin;
        }

        public bool Save()
        {
            return _context.SaveChanges() > 0;
        }

        public bool UserExists(string username)
        {
            return _context.Users.Any(u => u.Username == username);
        }

        public string GetPassword(string username)
        {
            return _context.Users.Where(p => p.Username == username).First().Password; //Get the password
        }

        public bool UserExistsId(int id)
        {
            return _context.Users.Any(u => u.User_Id == id);
        }

        public int DecodeToken(string token)
        { // Decodes the token and gets the User Id
            JwtSecurityTokenHandler tokenHandler = new(); // create the token handler
            JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(token); // read the token

            //Create a userIdString that gets the value of the ID stored in the token.
            string userIdString = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            //To be able to use it. We convert it to an integer
            _ = int.TryParse(userIdString, out int userIdInt);

            //and we return the integer.
            return userIdInt;
        }

        public string DecodeTokenRole(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new(); // Creates a new instance of the jwtsectokenhandl class
            JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(token);

            // Create a string that gets the value of the ROLE stored in the token
            string roleString = jwtToken.Claims.FirstOrDefault(r => r.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            // In this case we return a string
            return roleString;
        }

        public bool DeleteUser(User user)
        {
            if (ComparedUserIds(user.User_Id) || IsUserAdmin()) // Admins can delete any user. But they can delete other admins. Will fix that later
            {
                _context.Remove(user); // If User Ids are the same, delete
                return Save(); // And save
            }
            else
                return false; // If not, return false and get server error
        }

        public User GetUserById(int id)
        {
            return _context.Users.Where(u => u.User_Id == id).FirstOrDefault();
        }

        public bool ValidateEmail(string email)
        {
            string[] validateEmail = { ".com", ".net", ".org", ".edu", ".mil", ".co", ".us", ".io" }; // We validate if any of this values are present

            bool isEmailValid = false;

            foreach (var item in validateEmail)
            {
                if (email.Contains(item))
                {
                    isEmailValid = true;
                    break; // No need to iterate over the remaning values when we notice the email has a valid value
                }
            }

            return isEmailValid; // returns true if email is valid and false if email its 
        }

        public bool ComparedUserIds(int userId)
        {
            var user = _context.Users.Find(userId);

            var idFromToken = DecodeToken(GetToken()); // Get the user id from the token
            var idFromUser = user.User_Id; // Get the user id from the user object created above

            if (idFromUser != idFromToken)
                return false; // Will cause server error
            else
                return true; // If they do match, return true
        }

        public string GetToken()
        {
            string bearerToken = _accesor.HttpContext.Request.Headers.Authorization.ToString(); // Get the token from the Authorization Headers
            bearerToken = bearerToken.Replace("Bearer", "").Trim(); // Its gote some characters that we dont need. We remove them

            return bearerToken; // Return the token
        }

        public bool IsUserAdmin()
        {
            string userRoleFromToken = DecodeTokenRole(GetToken()).ToString();

            if (userRoleFromToken == "admin")
                return true; // User is an admin
            else
                return false; // User its not an admin
        }
    }
}
