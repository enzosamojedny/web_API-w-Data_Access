using Microsoft.AspNetCore.Mvc;
using BLL;
using Models.Entities;
using Microsoft.AspNetCore.Authorization;
using API.Controllers.DTOs;
using static System.Reflection.Metadata.BlobBuilder;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly ILogger<BookController> _logger;

        public BookController(IBusinessLayer businessLayer, ILogger<BookController> logger)
        {
            _businessLayer = businessLayer;
            _logger = logger;
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<BookDto>> CreateBook([FromBody] BookDto bookDto)
        {
            _logger.LogInformation("CreateBook called with title: {Title}", bookDto?.Titulo);

            try
            {
                if (bookDto == null)
                {
                    _logger.LogWarning("CreateBook received null BookDto.");
                    return StatusCode(StatusCodes.Status400BadRequest, "Invalid book data.");
                }

                var newBook = new Book
                {
                    Titulo = bookDto.Titulo,
                    Autor = bookDto.Autor,
                    Descripcion = bookDto.Descripcion,
                    FechaPublicacion = bookDto.FechaPublicacion
                };

                _logger.LogDebug("Attempting to create book: {@NewBook}", newBook);
                var createdBook = await _businessLayer.CreateBook(newBook);

                if (createdBook == null)
                {
                    _logger.LogError("Failed to create book: {Title}", bookDto.Titulo);
                    return StatusCode(StatusCodes.Status400BadRequest, $"Could not create book {bookDto.Titulo}.");
                }

                var createdBookDto = new BookDto
                {
                    Titulo = createdBook.Titulo,
                    Autor = createdBook.Autor,
                    Descripcion = createdBook.Descripcion,
                    FechaPublicacion = createdBook.FechaPublicacion
                };

                _logger.LogInformation("Book created successfully: {Title}", createdBookDto.Titulo);
                return CreatedAtAction(nameof(GetBooks), new { id = createdBook.ID }, createdBookDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a book.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks(int? id = null, string? titulo = null, string? autor = null)
        {
            try
            {
                var books = await _businessLayer.GetBooks(id, titulo, autor);

                if (books == null)
                {
                    _logger.LogWarning("No book found for the given criteria. ID: {Id}, Title: {Title}, Author: {Author}", id, titulo, autor);
                    return NotFound("Book not found.");
                }
                var bookDtos = new List<BookDto>();

                foreach (var book in books)
                {
                    bookDtos.Add(new BookDto
                    {
                        Titulo = book.Titulo,
                        Autor = book.Autor,
                        Descripcion = book.Descripcion,
                        FechaPublicacion = book.FechaPublicacion
                    });
                }
                return Ok(bookDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving a book.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}