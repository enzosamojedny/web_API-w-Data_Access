namespace API.Controllers.DTOs.Response
{
    public class BookLoanResponseDto
    {
        public int BookId { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public int UserId { get; set; }
    }
}
