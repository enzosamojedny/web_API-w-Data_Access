namespace Models.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string Descripcion { get; set; }
        public int? PublishedYear { get; set; }
        public int? UserId { get; set; }
    }
}
