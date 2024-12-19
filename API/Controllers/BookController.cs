using Microsoft.AspNetCore.Mvc;
using BLL;
using Models.Entities;
using Microsoft.AspNetCore.Authorization;
using API.Controllers.DTOs.Request;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly Login _login;

        public BookController(IBusinessLayer businessLayer, Login login)
        {
            _businessLayer = businessLayer;
            _login = login;
        }

        [HttpPost]
        public ActionResult<UserDto> CreateUser([FromBody] UserDto userDto)
        {
            try
            {
                if (userDto == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Invalid user data.");
                }

                var newUser = new User
                {
                    ID = userDto.ID,
                    Nombre = userDto.Nombre,
                    Edad = userDto.Edad,
                    Email = userDto.Email,
                    Deleted = userDto.Deleted ?? false,
                    Password = _login.EncryptSHA256(userDto.Password)
                };

                var createdUser = _businessLayer.CreateUser(newUser);

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
                    Deleted = createdUser.Deleted
                };

                return CreatedAtAction(nameof(_businessLayer.GetUser), new { id = createdUser.ID }, createdUserDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult<UserDto> GetUser(int? id = null, string? email = null, int? age = null, string? dni = null)
        {
            try
            {
                var user = _businessLayer.GetUser(id, email, age, dni);

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
                    Deleted = user.Deleted
                };
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}