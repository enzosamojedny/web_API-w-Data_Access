﻿namespace API.Controllers.DTOs
{
    public class RentBookDTO
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Status { get; set; }
    }
}