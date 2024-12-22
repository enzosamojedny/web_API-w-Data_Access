using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Entities;

namespace DAO
{
    public interface IDataAccess
    {
        Task<User> CreateUser(User user);
        Task<List<User>> GetAllUsers();
        Task<User> UpdateUser(User user);
        Task<User> GetUser(int? id = null, string email = null, int? edad = null, int? dni = null);
        bool SoftDeleteUser(int userID);
    }
}
