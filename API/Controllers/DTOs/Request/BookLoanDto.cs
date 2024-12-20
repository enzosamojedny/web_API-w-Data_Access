namespace API.Controllers.DTOs.Request
{
    public class BookLoanDto
    {
        public int BookId { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public int UserId { get; set; }
    }
}
