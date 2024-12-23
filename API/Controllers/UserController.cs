﻿using Microsoft.AspNetCore.Mvc;
using BLL;
using Models.Entities;
using Microsoft.AspNetCore.Authorization;
using API.Controllers.DTOs;

namespace API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly Login _login;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IBusinessLayer businessLayer, Login login, ILogger<UsersController> logger)
        {
            _businessLayer = businessLayer;
            _login = login;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserDto userDto)
        {
            try
            {
                if (userDto == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Invalid user data.");
                }

                //parseo el string -rol- recibido al ENUM de la entity
                if (!Enum.TryParse<Rol>(userDto.Rol, true, out var parsedRole))
                {
                    return BadRequest($"Invalid role. Allowed roles: {string.Join(", ", Enum.GetNames(typeof(Rol)))}");
                }

                var newUser = new User
                {
                    ID = userDto.ID,
                    Nombre = userDto.Nombre,
                    Edad = userDto.Edad,
                    Email = userDto.Email,
                    DNI = userDto.DNI,
                    Deleted = userDto.Deleted ?? false,
                    Password = _login.EncryptSHA256(userDto.Password),
                    Rol = parsedRole
                };

                var createdUser = await _businessLayer.CreateUser(newUser);

                if (createdUser == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, $"Could not create user {userDto.Nombre}.");
                }

                // eliminada info sensible en el mapeo a DTO
                var createdUserDto = new UserDto
                {
                    ID = createdUser.ID,
                    Nombre = createdUser.Nombre,
                    Edad = createdUser.Edad,
                    Email = createdUser.Email,
                    DNI = createdUser.DNI,
                    Rol = createdUser.Rol.ToString()
                };

                return CreatedAtAction(nameof(_businessLayer.GetUser), new { id = createdUser.ID }, createdUserDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPatch]
        public async Task<ActionResult<UserDto>> UpdateUser([FromBody] UserDto userDto)
        {
            try
            {
                var validUser = await _businessLayer.GetUser(userDto.ID.Value);
                if (validUser == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Invalid user data.");
                }

                if (!Enum.TryParse<Rol>(userDto.Rol, true, out var parsedRole))
                {
                    return BadRequest($"Invalid role. Allowed roles: {string.Join(", ", Enum.GetNames(typeof(Rol)))}");
                }

                var userToUpdate = new User
                {
                    ID = userDto.ID,
                    Nombre = userDto.Nombre,
                    Edad = userDto.Edad,
                    Email = userDto.Email,
                    DNI = userDto.DNI,
                    Deleted = userDto.Deleted ?? validUser.Deleted,
                    Password = validUser.Password,
                    Rol = parsedRole
                };

                var updatedUser = await _businessLayer.UpdateUser(userToUpdate);
                if (updatedUser == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, $"Could not update user with ID {userDto.ID}.");
                }

                var updatedUserDto = new UserDto
                {
                    ID = updatedUser.ID,
                    Nombre = updatedUser.Nombre,
                    Edad = updatedUser.Edad,
                    Email = updatedUser.Email,
                    DNI = updatedUser.DNI,
                    Deleted = updatedUser.Deleted,
                    Rol = updatedUser.Rol.ToString()
                };

                return StatusCode(StatusCodes.Status200OK, updatedUserDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            try
            {

                var users = await _businessLayer.GetAllUsers();

                if (users == null || users.Count == 0)
                {
                    return StatusCode(StatusCodes.Status404NotFound, "No users found.");
                }

                var userDtos = users.Select(user => new UserDto
                {
                    ID = user.ID,
                    Nombre = user.Nombre,
                    Edad = user.Edad,
                    Email = user.Email,
                    DNI = user.DNI,
                    Deleted = user.Deleted,
                    Rol = user.Rol.ToString()
                }).ToList();


                return StatusCode(StatusCodes.Status200OK, userDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("search")]
        public async Task<ActionResult<UserDto>> GetUser(int? id = null, string? email = null, int? age = null, int? dni = null)
        {
            try
            {
                var user = await _businessLayer.GetUser(id, email, age, dni);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var userDto = new UserDto
                {
                    ID = user.ID,
                    Nombre = user.Nombre,
                    Edad = user.Edad,
                    Email = user.Email,
                    DNI = user.DNI
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            //caso de que este vacio alguno de los dos campos
            if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest(new { isSuccess = false, message = "Email and password are required" });
            }

            //busco un usuario con esos datos en la db
            var validUser =  await _businessLayer.GetUser(null, login.Email);

            //si no se encontro nada...
            if (validUser == null)
            {
                return BadRequest(new { isSuccess = false, message = "Invalid credentials" });
            }
            string inputPassword = _login.EncryptSHA256(login.Password);

            _logger.LogInformation($"hash guardado: {validUser.Password}");
            _logger.LogInformation($"input hash: {inputPassword}");

            if (validUser.Password != inputPassword)
            {
                return BadRequest(new { isSuccess = false, token = "" });
            }

            //genero el jwt si salio todo ok
            var token = _login.GenerateJWT(validUser);

            return Ok(new { isSuccess = true, token });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteUser(int id)
        {
            try
            {
                var validID = await _businessLayer.GetUser(id);

                bool isDeleted = _businessLayer.SoftDeleteUser(id);

                if (!isDeleted)
                {
                    return NotFound($"User with ID {id} not found or could not be deleted.");
                }

                return Ok($"User with ID {id} has been successfully deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while attempting to soft delete user with ID {id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the user.");
            }
        }

    }
}