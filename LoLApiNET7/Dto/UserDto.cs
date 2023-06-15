namespace LoLApiNET7.Dto
{
    public class UserDto
    {
        public required string Username { get; set; } //username is required
        public required string Password { get; set; } //password is required. Not the hashed one
    }
}
