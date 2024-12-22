using Dapper;
using MySqlConnector;
using Microsoft.Extensions.Logging;
using Models.Entities;
using System.Text;

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
                        INSERT INTO Users (Nombre, Edad, Email,DNI, Password, Rol)
                        SELECT @Nombre, @Edad, @Email,@DNI, @Password, COALESCE(@Rol, 'User')
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
                                Rol = user.Rol
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

                //return connection.Query<User>("SELECT * FROM Users WHERE Deleted = 0").ToList();

            const string query = @"
            SELECT 
                ID,
                Nombre,
                Edad,
                Email,
                DNI,
                Deleted,
                Rol,
                Password
            FROM Users 
            WHERE Deleted = 0";

            //solucionar problema con mapping de string a enum

                return connection.Query<User>(query).ToList();
            }
        }

        public async Task<User> GetUser(int? id = null, string email = null, int? edad = null, int? dni = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT ID, Nombre, Edad, Email, DNI, Password FROM Users WHERE Deleted = 0";
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
                        _logger.LogError(ex, "Error in soft delete user");
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
                        int rowsAffected = connection.Execute(
                            @"UPDATE Users 
                            SET Nombre = @Nombre, Edad = @Edad, Email = @Email, DNI = @DNI, Rol = @Rol, Password = @Password, Deleted = @Deleted 
                            WHERE ID = @ID AND Deleted = FALSE""",
                            user,
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

    }
}