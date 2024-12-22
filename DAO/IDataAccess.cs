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
        User CreateUser(User user);
        List<User> GetAllUsers();
        User UpdateUser(User user);
        User GetUser(int? id = null, string email = null, int? edad = null, int? dni = null);
        bool SoftDeleteUser(int userID);

        //IEnumerable<User> GetUsers(); //???
    }
}
