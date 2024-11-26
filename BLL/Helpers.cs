using System.Text.RegularExpressions;

namespace Helpers
{
    public static class UserValidator
    {
        public static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            }

            if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@gmail\.com$", RegexOptions.IgnoreCase))
                throw new ArgumentException("Email must be a valid Gmail address");

        }
        public static void ValidateAge(int age, int minimumAge = 14)
        {
            if (age < minimumAge)
            {
                throw new ArgumentException($"Age must be at least {minimumAge} to proceed.");
            }
        }
    }
}
