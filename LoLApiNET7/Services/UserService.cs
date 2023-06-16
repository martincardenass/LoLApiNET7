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
        string GetPassword(string username);
        User Authenticate(UserDto user);
        //string Login(UserDto user);
        //User EncryptPassword(User user);
        bool UserExists(string username);
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
                new Claim(ClaimTypes.NameIdentifier, u.Username),
                new Claim(ClaimTypes.Email, u.Email),
                new Claim(ClaimTypes.Role, u.Role)
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15), // Expires in 15 minutes
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
        //public string Login(UserDto user)
        //{
        //    var logged = Authenticate(user);

        //    var token = CreateToken(logged);

        //    return token;
        //}
    }
}
