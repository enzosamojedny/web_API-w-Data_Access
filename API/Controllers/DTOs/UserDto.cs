namespace API.Controllers.DTOs
{
    public class UserDto
    {
        public int? ID { get; set; }
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public string Email { get; set; }
        public int DNI { get; set; }
        public bool? Deleted { get; set; } = false;
        public string Rol { get; set; }
        public string? Password { get; set; }
    }
}
