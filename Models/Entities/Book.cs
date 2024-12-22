namespace Models.Entities
{
    public class Book
    {
        public int ID { get; set; }
        public string Titulo { get; set; }
        public string? Autor { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaPublicacion { get; set; }
        public int? UserId { get; set; } //
    }
}
