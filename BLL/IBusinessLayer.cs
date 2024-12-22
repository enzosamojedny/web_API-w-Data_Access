using Models.Entities;

namespace BLL
{
    public interface IBusinessLayer
    {
        Task<User> CreateUser(User user);
        Task<List<User>> GetAllUsers();
        Task<User> UpdateUser(User user);
        Task<User> GetUser(int? id = null, string email = null, int? edad = null, int? dni = null);
        bool SoftDeleteUser(int userID);
    }
}
