﻿namespace API.Controllers.DTOs.Response
{
    public class BookLoanResponseDto
    {
        public int BookId { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int UserId { get; set; }
    }
}
