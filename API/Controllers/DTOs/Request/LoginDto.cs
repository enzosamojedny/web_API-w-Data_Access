namespace API.Controllers.DTOs.Request
{
    public class LoginDto
    {
        public int ID { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; } //varchar 20, hacer un check
    }
}
