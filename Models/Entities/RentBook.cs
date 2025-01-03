namespace Models.Entities
{
    public class RentBook
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public RentBookStatus Status { get; set; } = RentBookStatus.Disponible;
    }
}
