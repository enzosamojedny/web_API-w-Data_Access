    using Helpers;
    using Models.Entities;
    using DAO;
    using Microsoft.Extensions.Logging;
using Models.DTOs;
using Repository_Layer;

namespace BLL
{
    public class BusinessLayer : IBusinessLayer
    {
        private readonly IUserRepository _methods;
        //private readonly string _connectionString = "Server=localhost;Port=3306;Database=extradosdb;User ID=root;Password=123456;";

        public BusinessLayer(IUserRepository methods)
        {
            _methods = methods;
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

            return _methods.CreateUser(user);
        }
        public List<UserDto> GetAllUsers() => _methods.GetAllUsers();
        public User GetUserByID(int id)
        {
            return _methods.GetUserByID(id);
        }
        public UserDto GetUserByEmail(string email)
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