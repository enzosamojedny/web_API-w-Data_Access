using Dapper;
using MySqlConnector;
using Microsoft.Extensions.Logging;
using Models.Entities;


namespace DAO
{

    public class DataAccess : IDataAccess
    {

        private readonly string _connectionString;
        private readonly ILogger _logger;

        public DataAccess(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }
        public async Task<User> CreateUser(User user)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string insertQuery = @"
                        INSERT INTO Users (Nombre, Edad, Email, DNI, Password, Rol)
                        SELECT @Nombre, @Edad, @Email,@DNI, @Password, @Rol
                        WHERE @Edad >= 14;
                        SELECT LAST_INSERT_ID();";

                        int newUserId = connection.ExecuteScalar<int>(
                            insertQuery,
                            new
                            {
                                Nombre = user.Nombre,
                                Edad = user.Edad,
                                Email = user.Email,
                                DNI = user.DNI,
                                Password = user.Password,
                                Rol = user.Rol.ToString()
                            },
                            transaction
                        );

                        transaction.Commit();
                        return await GetUser(newUserId);
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine($"Database error: {ex.Message}");
                        transaction.Rollback();
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unexpected error: {ex.Message}");
                        transaction.Rollback(); // rollback unknown error
                        throw;
                    }
                }
            }
        }

        public async Task<List<User>> GetAllUsers()
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                const string query = @"
            SELECT 
                Nombre,
                Edad,
                Email,
                DNI,
                Deleted,
                Rol
            FROM Users 
            WHERE Deleted = 0";

                return connection.Query<User>(query).ToList();
            }
        }

        public async Task<User> GetUser(int? id = null, string email = null, int? edad = null, int? dni = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT ID, Nombre, Edad, Email, DNI, Password, Rol FROM Users WHERE Deleted = 0";
            var where = new List<string>();
            var parameters = new { Id = id, Email = email, Edad = edad, DNI = dni };

            if (id.HasValue) where.Add("ID = @Id");
            if (!string.IsNullOrEmpty(email)) where.Add("Email = @Email");
            if (edad.HasValue) where.Add("Edad = @Edad");
            if (dni.HasValue) where.Add("DNI = @DNI");

            if (where.Any())
                query += " AND " + string.Join(" AND ", where);

            return connection.QuerySingleOrDefault<User>(query, parameters);
        }

        public bool SoftDeleteUser(int userID)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int rowsAffected = connection.Execute(
                            "UPDATE Users SET Deleted = 1 WHERE ID = @userID",
                            new { userID },
                            transaction
                        );

                        if (rowsAffected > 0)
                        {
                            transaction.Commit();
                            return true;
                        }
                        transaction.Rollback();
                        return false;
                    }
                    catch (MySqlException ex)
                    {
                        _logger.LogError(ex, "Error soft deleting user");
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public async Task<User> UpdateUser(User user)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var parameters = new
                        {
                            user.ID,
                            user.Nombre,
                            user.Edad,
                            user.Email,
                            user.DNI,
                            Rol = user.Rol.ToString(),
                            user.Password,
                            user.Deleted
                        };

                        int rowsAffected = connection.Execute(
                            @"UPDATE Users 
                      SET Nombre = @Nombre, Edad = @Edad, Email = @Email, DNI = @DNI, Rol = @Rol, Password = @Password, Deleted = @Deleted 
                      WHERE ID = @ID AND Deleted = FALSE",
                            parameters,
                            transaction
                        );

                        if (rowsAffected == 0)
                        {
                            transaction.Rollback();
                            return null;
                        }

                        var updatedUser = connection.QuerySingleOrDefault<User>(
                            "SELECT ID, Nombre, Edad, Email, DNI, Rol, Deleted FROM Users WHERE ID = @ID AND Deleted = 0",
                            new { user.ID },
                            transaction
                        );

                        transaction.Commit();
                        return updatedUser;
                    }
                    catch (MySqlException ex)
                    {
                        _logger.LogError(ex, "Error updating user");
                        transaction.Rollback();
                        return null;
                    }
                }
            }
        }

        public async Task<Book> CreateBook(Book book)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var query = @"INSERT INTO Books (Titulo, Autor, Descripcion, FechaPublicacion, UserId) 
                              VALUES (@Titulo, @Autor, @Descripcion, @FechaPublicacion, @UserId);
                              SELECT LAST_INSERT_ID();";

                        var newId = await connection.ExecuteScalarAsync<int>(query, book, transaction);
                        book.ID = newId;

                        transaction.Commit();
                        return book;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError(ex, "Error creating book");
                        throw;
                    }
                }
            }
        }

        public async Task<IEnumerable<Book>> GetBooks(int? id = null, string? titulo = null, string? autor = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT ID, Titulo, Autor, Descripcion, FechaPublicacion, UserId FROM Books";
                var where = new List<string>();
                var parameters = new DynamicParameters();

                if (id.HasValue)
                {
                    where.Add("ID = @Id");
                    parameters.Add("Id", id);
                }
                if (!string.IsNullOrEmpty(titulo))
                {
                    where.Add("Titulo = @Titulo");
                    parameters.Add("Titulo", titulo);
                }
                if (!string.IsNullOrEmpty(autor))
                {
                    where.Add("Autor = @Autor");
                    parameters.Add("Autor", autor);
                }

                if (where.Any())
                    query += " WHERE " + string.Join(" AND ", where);
                return await connection.QueryAsync<Book>(query, parameters);
            }
        }

        public async Task<Book> RentBook(string email, int bookId)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            {
                var userExists = await GetUser(null, email);
                try
                {
                    //existe el usuario?


                    if (userExists == null)
                    {
                        throw new InvalidOperationException("User does not exist.");
                    }

                    //el libro esta disponible para alquilar?
                    var isAvailable = await connection.ExecuteScalarAsync<bool>(
                        @"SELECT COUNT(*) > 0 
                        FROM RentBooks 
                        WHERE BookId = @BookId AND Status = @Status FOR UPDATE;",
                        //for update me lockea la transaccion, para que no se guarde si sale mal
                        new { BookId = bookId, Status = RentBookStatus.Disponible },
                        transaction);

                    if (!isAvailable)
                    {
                        throw new InvalidOperationException("Book is not available for rent.");
                    }

                    //si esta disponible, lo alquilo
                    var rentQuery = @"INSERT INTO RentBooks (UserId, BookId, FechaPrestamo, FechaVencimiento, Status) 
                              VALUES (@UserId, @BookId, @FechaPrestamo, @FechaVencimiento, @Status)";

                    var rent = new
                    {
                        UserId = userExists.ID,
                        BookId = bookId,
                        FechaPrestamo = DateTime.UtcNow,
                        FechaVencimiento = DateTime.UtcNow.AddDays(5),
                        Status = RentBookStatus.Activo
                    };

                    await connection.ExecuteAsync(rentQuery, rent, transaction);

                    // actualizo el userId de books para mostrar que esta alquilado
                    await connection.ExecuteAsync(
                        "UPDATE Books SET UserId = @UserId WHERE ID = @BookId",
                        new { UserId = userExists.ID, BookId = bookId },
                        transaction);

                    transaction.Commit();

                    //devuelvo el libro alquilado como response
                    var bookQuery = @"SELECT ID, Titulo, Autor, Descripcion, FechaPublicacion, UserId 
                              FROM Books 
                              WHERE ID = @BookId";

                    var rentedBook = await connection.QuerySingleOrDefaultAsync<Book>(bookQuery, new { BookId = bookId });

                    if (rentedBook == null)
                    {
                        throw new InvalidOperationException("Failed to retrieve rented book details.");
                    }
                    return rentedBook;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Error renting book for UserId: {UserId}, BookId: {BookId}", userExists.ID, bookId);
                    throw;
                }
            }
        }
    }
}