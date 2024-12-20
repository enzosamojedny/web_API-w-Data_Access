using DAO;
using Helpers;
    using Models.Entities;
using System.Text.RegularExpressions;

namespace BLL
{
    public class BusinessLayer : IBusinessLayer
    {
        private readonly IDataAccess _methods;

        public BusinessLayer(IDataAccess methods)
        {
            _methods = methods;
        }

        public User CreateUser(User user)
        {
            UserValidator.ValidateEmail(user.Email);
            UserValidator.ValidateAge(user.Edad);
            UserValidator.ValidateDNI(user.DNI.ToString());

            var existingUser = GetUser(null,user.Email);
            if (existingUser != null)
            {
                throw new Exception("An user with this email already exists.");
            }

            return _methods.CreateUser(user);
        }
        public List<User> GetAllUsers() => _methods.GetAllUsers();

        public User GetUser(int? id = null, string? email = null, int? age = null, string? dni = null)
        {
            UserValidator.ValidateEmail(email);
            UserValidator.ValidateAge(age);
            UserValidator.ValidateDNI(dni);

            int validatedDNI = int.Parse(dni);

            return _methods.GetUser(id, email, age, validatedDNI);
        }

        public User GetUserByID(int id)
        {
            return _methods.GetUser(id);
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