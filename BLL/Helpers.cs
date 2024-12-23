using Models.Entities;
using System.Text.RegularExpressions;

namespace Helpers
{
    public static class UserValidator
    {
        public static void ValidateEmail(string? email)
        {
            if (email == null) throw new ArgumentNullException("Must provide an email address");
            if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@gmail\.com$", RegexOptions.IgnoreCase))
                throw new ArgumentException("Email must be a valid Gmail address");
        }
        public static void ValidateAge(int? age, int minimumAge = 14)
        {
            if (age < minimumAge)
            {
                throw new ArgumentException($"Age must be at least {minimumAge} to proceed.");
            }
        }
        public static void ValidateDNI(string? dni)
        {
            if (string.IsNullOrEmpty(dni))
            {
                throw new ArgumentNullException(nameof(dni), "DNI cannot be null or empty");
            }

            if (dni.Length < 6 || dni.Length > 9)
            {
                throw new ArgumentException($"DNI length is incorrect");
            }
            
            if (Regex.IsMatch(dni, @"[^0-9]"))
            {
                throw new ArgumentException("DNI must contain only numbers");
            }
        }

        public static void ValidateEnum(string? enumValue) 
        {
            if (!Enum.IsDefined(typeof(Rol), enumValue))
            {
                throw new ArgumentException($"Invalid role: {enumValue}");
            }
        }

    }
}
