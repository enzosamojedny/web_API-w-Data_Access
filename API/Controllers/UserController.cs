using Microsoft.AspNetCore.Mvc;
using BLL;
using Models.Entities;
using Microsoft.AspNetCore.Authorization;
using API.Controllers.DTOs.Request;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly Login _login;

        public UsersController(IBusinessLayer businessLayer, Login login)
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
        [HttpPatch]
        public ActionResult<UserDto> UpdateUser([FromBody] UserDto userDto)
        {
            try
            {
                var validUser = _businessLayer.GetUser(userDto.ID.Value);

                if (validUser == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Invalid user data.");
                }

                // mapeo normal
                var userToUpdate = new User
                {
                    ID = userDto.ID,
                    Nombre = userDto.Nombre,
                    Edad = userDto.Edad,
                    Email = userDto.Email,
                    Deleted = userDto.Deleted ?? validUser.Deleted,
                    Password = validUser.Password
                };

                var updatedUser = _businessLayer.UpdateUser(userToUpdate);

                if (updatedUser == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, $"Could not update user with ID {userDto.ID}.");
                }

                // mapeo a DTO
                var updatedUserDto = new UserDto
                {
                    ID = updatedUser.ID,
                    Nombre = updatedUser.Nombre,
                    Edad = updatedUser.Edad,
                    Email = updatedUser.Email,
                    Deleted = updatedUser.Deleted
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
        public ActionResult<List<UserDto>> GetAllUsers()
        {
            try
            {
                var users = _businessLayer.GetAllUsers();

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
                    Deleted = user.Deleted
                }).ToList();

                return StatusCode(StatusCodes.Status200OK, userDtos);
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
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            var validUser = _businessLayer.GetUser(null,login.Email);

            if(validUser.Email == null || validUser.Password == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { isSuccess = false, token = "" });
            }

            var user = new User
            {
                Email = login.Email,
                Password = login.Password
            };
            var usuarioEncontrado = await _businessLayer.GetUser(login.Email && _login.GenerateJWT(login.Password));

            if (usuarioEncontrado == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { isSuccess = false, token = "" });
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, token = _login.GetHashCode(usuarioEncontrado) });
            }

        }
    }
}