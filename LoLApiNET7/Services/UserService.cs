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
        string CreateToken(User user);
        int DecodeToken(string token);
        //string DecodeToken(JwtSecurityToken securityToken);
        string GetPassword(string username);
        User Authenticate(UserDto user);
        //string Login(UserDto user);
        //User EncryptPassword(User user);
        bool UserExists(string username);
        bool UserExistsId(int id); //check if user exists by its Id. Created to use in the reviews.
        bool EmailExists(string email);
        bool CreateUser(User user);
        bool Save();
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UserService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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

        //public User EncryptPassword(User user)
        //{
        //    string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password); //Encrypt the user's password

        //    //user.Username = user.Username;
        //    user.Password = passwordHash;

        //    return user;
        //}

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
    }
}
