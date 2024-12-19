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
            _connectionString = "Server=localhost;Port=3306;Database=extradosdb;User ID=test;Password=123456;";
            _logger = logger;
        }
        public User CreateUser(User user)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string insertQuery = @"
                        INSERT INTO usuarios (Nombre, Edad, Email, Password)
                        SELECT @Nombre, @Edad, @Email, @Password
                        WHERE @Edad >= 14;
                        SELECT LAST_INSERT_ID();";

                        int newUserId = connection.ExecuteScalar<int>(
                            insertQuery,
                            new { Nombre = user.Nombre, Edad = user.Edad, Email = user.Email, Password = user.Password },
                            transaction
                        );

                        transaction.Commit(); // Commit

                       
                        return GetUser(newUserId);
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine($"Database error: {ex.Message}");
                        transaction.Rollback(); // Rollback
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unexpected error: {ex.Message}");
                        transaction.Rollback(); // Rollback unknown error
                        throw;
                    }
                }
            }
        }

        public List<User> GetAllUsers()
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                return connection.Query<User>("SELECT * FROM Usuarios WHERE Deleted = 0").ToList();
            }
        }

        public User GetUser(int? id = null, string email = null, int? edad = null, int? dni = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT ID, Nombre, Edad, Email FROM usuarios WHERE Deleted = 0";
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
                            "UPDATE usuarios SET Deleted = 1 WHERE ID = @userID",
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

        public User UpdateUser(User user)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Update user
                        int rowsAffected = connection.Execute(
                            @"UPDATE usuarios 
                            SET Nombre = @Nombre, Edad = @Edad, Email = @Email, Deleted = @Deleted 
                            WHERE ID = @ID",
                            user,
                            transaction
                        );

                        if (rowsAffected == 0)
                        {
                            transaction.Rollback();
                            return null;
                        }

                        var updatedUser = connection.QuerySingleOrDefault<User>(
                            "SELECT ID, Nombre, Edad, Email, Deleted FROM usuarios WHERE ID = @ID AND Deleted = 0",
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