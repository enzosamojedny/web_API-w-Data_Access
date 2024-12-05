using Dapper;
using MySqlConnector;
using Microsoft.Extensions.Logging;
using Models.Entities;
using Models.DTOs;

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

                        transaction.Commit(); // Commit the transaction

                        //return GetUserByID(newUserId, connection, transaction); // Use overloaded GetUserByID
                        return GetUserByID(newUserId);
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine($"Database error: {ex.Message}");
                        transaction.Rollback(); // Rollback on failure
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unexpected error: {ex.Message}");
                        transaction.Rollback(); // Rollback on unknown error
                        throw;
                    }
                }
            }
        }

        public List<UserDto> GetAllUsers()
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                return connection.Query<UserDto>("SELECT * FROM Usuarios WHERE Deleted = 0").ToList();
            }
        }

        //porque estoy enviando todo esto por parametros???
        //public User GetUserByID(int id, MySqlConnection connection = null, IDbTransaction transaction = null)
        //{
        //    bool closeConnection = connection == null;
        //    connection ??= new MySqlConnection(_connectionString);

        //    if (closeConnection)
        //        connection.Open();

        //    try
        //    {
        //        string query = "SELECT ID, Nombre, Edad, Email FROM usuarios WHERE ID = @ID AND Deleted = 0";
        //        return connection.QuerySingleOrDefault<User>(query, new { ID = id }, transaction);
        //    }
        //    finally
        //    {
        //        if (closeConnection)
        //            connection.Close();
        //    }
        //}
        public User GetUserByID(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                return connection.QuerySingleOrDefault<User>(
                    "SELECT ID, Nombre, Edad, Email FROM usuarios WHERE ID = @Id AND Deleted = 0",
                    new { Id = id }
                );
            }
        }
        public UserDto GetUserByEmail(string email)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                return connection.QuerySingleOrDefault<UserDto>(
                    "SELECT ID, Nombre, Edad, Email FROM usuarios WHERE Email = @email AND Deleted = 0",
                    new { email }
                );
            }
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