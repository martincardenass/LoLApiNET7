using System.ComponentModel.DataAnnotations;

namespace LoLApiNET7.Models
{
    public class User
    {
        [Key]
        public int User_Id { get; set; }
        public string Username { get; set; }
        public DateTime Created { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
    }
}
