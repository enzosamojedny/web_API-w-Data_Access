using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.DTOs;
using Models.Entities;

namespace BLL
{
    public interface IBusinessLayer
    {
        User CreateUser(User user);
        User GetUserByID(int id);
        UserDto GetUserByEmail(string email);
        List<UserDto> GetAllUsers();
        User UpdateUser(User user);
        bool SoftDeleteUser(int userID);
    }
}
