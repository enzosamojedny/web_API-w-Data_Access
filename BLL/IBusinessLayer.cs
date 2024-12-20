using Models.Entities;

namespace BLL
{
    public interface IBusinessLayer
    {
        User CreateUser(User user);
        List<User> GetAllUsers();
        User UpdateUser(User user);
        User GetUser(int? id = null, string email = null, int? edad = null, int? dni = null);
        bool SoftDeleteUser(int userID);
    }
}
