using DAO;
using Microsoft.Extensions.Logging;
using Models.DTOs;
using Models.Entities;
using Repository_Layer;


namespace Repository
{

    public class UserRepository : IUserRepository
    {
        private readonly IDataAccess _dataAccess;

        public UserRepository(IDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }
        public UserRepository(ILogger logger)
            : this(new DataAccess("Server=localhost;Port=3306;Database=extradosdb;User ID=test;Password=123456;", logger))
        { }


        public User CreateUser(User user) => _dataAccess.CreateUser(user);
        public List<UserDto> GetAllUsers() => _dataAccess.GetAllUsers();
        public UserDto GetUserByEmail(string email) => _dataAccess.GetUserByEmail(email);
        public User GetUserByID(int id) => _dataAccess.GetUserByID(id);
        public bool SoftDeleteUser(int userID) => _dataAccess.SoftDeleteUser(userID);
        public User UpdateUser(User user) => _dataAccess.UpdateUser(user);
    }
}
