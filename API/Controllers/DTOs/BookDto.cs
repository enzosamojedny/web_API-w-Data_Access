namespace API.Controllers.DTOs
{
    public class BookDto
    {
     public string Titulo { get; set; }
     public string? Autor { get; set; }
     public string? Descripcion { get; set; }
     public DateTime? FechaPublicacion { get; set; }
    }
}

