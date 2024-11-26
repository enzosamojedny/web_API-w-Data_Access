namespace DAO
{

    using MySqlConnector;
    using Models;
    public class Methods
    {

        //agregar validaciones tipo CHECK Edad mayor a 14
        public readonly string _connectionString;
        public Methods(string connectionString)
        {
            _connectionString = connectionString;
        }
        public User CreateUser(string nombre, int edad, string email)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(
                        @"INSERT INTO usuarios (Nombre, Edad, Email)
                        SELECT @nombre, @edad, @email
                        WHERE @edad > 14; 
                        SELECT LAST_INSERT_ID();",
                        connection))
                    {
                        command.Parameters.AddWithValue("@nombre", nombre);
                        command.Parameters.AddWithValue("@edad", edad);
                        command.Parameters.AddWithValue("@email", email);

                        int newUserId = Convert.ToInt32(command.ExecuteScalar());

                        return GetUserByID(newUserId);
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"Database error: {ex.Message}");
                    throw;
                }
            }
        }
        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand("SELECT * FROM Usuarios WHERE Deleted = 0", connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            User user = new User
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Nombre = reader["Nombre"].ToString(),
                                Edad = Convert.ToInt32(reader["Edad"])
                            };
                            users.Add(user);
                        }
                    }
                    connection.Close();
                    return users;
                }
            }
        }
        public User GetUserByID(int id)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand("SELECT ID,Nombre,Edad,Email FROM usuarios WHERE ID = @ID AND DELETED = 0", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        return new User
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            Nombre = reader["Nombre"].ToString(),
                            Edad = Convert.ToInt32(reader["Edad"])
                        };
                    }
                }
            }
            return null;
        }
        public User GetUserByEmail(string email)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand("SELECT ID,Nombre,Edad,Email FROM usuarios WHERE Email = @email AND DELETED = 0", connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Nombre = reader["Nombre"].ToString(),
                                Edad = Convert.ToInt32(reader["Edad"]),
                                Email = reader["Email"].ToString(),
                            };
                        }
                    };
                }
            }
            return null;
        }
        public bool SoftDeleteUser(int userID)
        {
            var userFound = GetUserByID(userID);
            if (userFound == null)
            {
                Console.WriteLine("UserID was incorrect");
                return false;
            }

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand("UPDATE usuarios SET Deleted = 1 WHERE ID = @userID;", connection))
                    {
                        command.Parameters.AddWithValue("@userID", userID);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
        public User UpdateUser(User user)
        {

            if (user == null)
            {
                Console.WriteLine("Email is invalid");
                throw new Exception("User does not exist.");
            }

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                using (MySqlCommand updateCommand = new MySqlCommand("UPDATE usuarios SET Nombre = @nombre, Edad = @edad WHERE ID = @id AND DELETED = 0", connection))
                {
                    updateCommand.Parameters.AddWithValue("@nombre", user.Nombre);
                    updateCommand.Parameters.AddWithValue("@edad", user.Edad);
                    updateCommand.Parameters.AddWithValue("@id", user.ID);

                    int rowsAffected = updateCommand.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        Console.WriteLine("No se pudo actualizar el usuario. Verifique el ID.");
                        return null;
                    }
                }

                using (MySqlCommand selectCommand = new MySqlCommand("SELECT Nombre, Edad FROM usuarios WHERE ID = @id AND DELETED = 0", connection))
                {
                    selectCommand.Parameters.AddWithValue("@id", user.ID);

                    using (MySqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return null;
                        }
                    }
                }
            }
            return null;
        }

    }
}