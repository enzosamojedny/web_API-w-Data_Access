﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Entities;
using MySqlConnector;

namespace DAO
{
    public interface IDataAccess
    {
        Task<User> CreateUser(User user);
        Task<List<User>> GetAllUsers();
        Task<User> UpdateUser(User user);
        Task<User> GetUser(int? id = null, string email = null, int? edad = null, int? dni = null);
        bool SoftDeleteUser(int userID);
        Task<IEnumerable<Book>> GetBooks(int? id = null, string? titulo = null, string? autor = null);

        Task<Book>CreateBook(Book book);
        //alquilar libro
        Task<Book> RentBook(string email, int bookId);
    }
}
