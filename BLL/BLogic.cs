namespace BLL
{
    using DAO;
    using Models;
    using Helpers;
    public class BusinessLayer
    {
        private readonly Methods _methods;
        private readonly string _connectionString = "Server=localhost;Port=3306;Database=extradosdb;User ID=root;Password=123456;";

        public BusinessLayer()
        {
            _methods = new Methods(_connectionString);
        }
        public User CreateUser(User user)
        {
            UserValidator.ValidateEmail(user.Email);
            UserValidator.ValidateAge(user.Edad);

            var existingUser = GetUserByEmail(user.Email);
            if (existingUser != null)
            {
                throw new Exception("An user with this email already exists.");
            }
            return _methods.CreateUser(user.Nombre, user.Edad, user.Email);
        }
        public List<User> GetAllUsers() => _methods.GetAllUsers();
        public User GetUserByID(int id)
        {
            return _methods.GetUserByID(id);
        }
        public User GetUserByEmail(string email)
        {
            //guard clauses
            UserValidator.ValidateEmail(email);

            return _methods.GetUserByEmail(email);
        }
        public bool SoftDeleteUser(int userID)
        {
            return _methods.SoftDeleteUser(userID);
        }
        public User UpdateUser(User user)
        {
            try
            {
                UserValidator.ValidateAge(user.Edad);
                UserValidator.ValidateEmail(user.Email);
                return _methods.UpdateUser(user);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}