namespace API.Controllers.DTOs
{
    public class RentBookDTO
    {
        public int UserId { get; set; }
        public string Titulo { get; set; }
        public DateTime FechaPrestamo { get; set; }

        public string Status { get; set; }
    }
}
