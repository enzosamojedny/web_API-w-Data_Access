using Models.Entities;
namespace Validation_Layer
{
    public interface IUserValidator
    {
        void ValidateForCreation(User user);
        void ValidateForUpdate(User user);
        bool IsEmailUnique(string email);
    }
}
