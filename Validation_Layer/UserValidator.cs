using System.ComponentModel.DataAnnotations;
using DAO;
using Models.DTOs;
using Models.Entities;

namespace Validation_Layer
{
    public class UserValidator : IUserValidator
    {
        private readonly IDataAccess _userDao;

        public UserValidator(IDataAccess userDao)
        {
            _userDao = userDao;
        }

        public void ValidateForCreation(User user)
        {
            // Check for null
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null");

            // Validate name
            if (string.IsNullOrWhiteSpace(user.Nombre))
                throw new ValidationException("Name cannot be empty");

            // Validate email format
            if (!IsValidEmail(user.Email))
                throw new ValidationException("Invalid email format");

            // Check email uniqueness
            if (!IsEmailUnique(user.Email))
                throw new ValidationException("Email is already in use");

            // Validate age
            ValidateAge(user.Edad);
        }

        public void ValidateForUpdate(User user)
        {
            // Similar to creation, but may have different rules
            if (user.ID <= 0)
                throw new ValidationException("Invalid user ID");

            ValidateForCreation(user);
        }

        public bool IsEmailUnique(string email)
        {
            // Check if email already exists in the system
            return _userDao.GetUserByEmail(email) == null;
        }

        public void ValidateAge(int age)
        {
            if (age < 14)
                throw new ValidationException("User must be at least 14 years old");

            if (age > 120)
                throw new ValidationException("Invalid age");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
