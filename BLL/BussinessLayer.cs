﻿using DAO;
using Helpers;
    using Models.Entities;
using System.Text.RegularExpressions;

namespace BLL
{
    public class BusinessLayer : IBusinessLayer
    {
        private readonly IDataAccess _methods;

        public BusinessLayer(IDataAccess methods)
        {
            _methods = methods;
        }

        public async Task<User> CreateUser(User user)
        {
            UserValidator.ValidateEmail(user.Email);
            UserValidator.ValidateAge(user.Edad);
            UserValidator.ValidateDNI(user.DNI.ToString());
            UserValidator.ValidateEnum(user.Rol.ToString());
            var existingUser = await GetUser(null,user.Email);
            if (existingUser != null)
            {
                throw new Exception("An user with this email already exists.");
            }

            return await _methods.CreateUser(user);
        }
        public async Task<List<User>> GetAllUsers() => await _methods.GetAllUsers();

        public async Task<User> GetUser(int? id = null, string? email = null, int? age = null, int? dni = null)
        {
            //si mando un id solo, debe aceptarlo y retornarme el usuario que pedi
            if (id.HasValue) return await _methods.GetUser(id);

            if (!string.IsNullOrEmpty(email))
            {
                UserValidator.ValidateEmail(email);
                return await _methods.GetUser(null, email, null, null);
            }

            if (dni.HasValue)
            {
                UserValidator.ValidateDNI(dni.Value.ToString());
                return await _methods.GetUser(null, null, null, dni);
            }

            if (dni.HasValue)
            {
                UserValidator.ValidateDNI(dni.Value.ToString());
            }
            return await _methods.GetUser(id, email, age, dni);
        }

        public bool SoftDeleteUser(int userID)
        {
            return _methods.SoftDeleteUser(userID);
        }
        public async Task<User> UpdateUser(User user)
        {
            try
            {
                UserValidator.ValidateAge(user.Edad);
                UserValidator.ValidateEmail(user.Email);
                return await _methods.UpdateUser(user);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}