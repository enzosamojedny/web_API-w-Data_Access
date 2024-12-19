namespace API.Controllers.DTOs.Request
{
    public class BookLoanDto
    {
        public int BookId { get; set; }
        public DateTime LoanDate { get; set; }
        public int UserId { get; set; }
    }
}
