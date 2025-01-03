using Models.Entities;

namespace BLL
{
    public interface IBusinessLayer
    {
        Task<User> CreateUser(User user);
        Task<List<User>> GetAllUsers();
        Task<User> UpdateUser(User user);
        Task<User> GetUser(int? id = null, string email = null, int? edad = null, int? dni = null);
        bool SoftDeleteUser(int userID);
        //libros
        Task<IEnumerable<Book>> GetBooks(int? id = null, string? titulo = null, string? autor = null);

        Task<Book> CreateBook(Book book);
        //alquilar libro
        Task<Book> RentBook(string email, int bookId);
    }
}

