using Microsoft.AspNetCore.Mvc;
using BLL;
using Models.Entities;
using Models.DTOs;
using Microsoft.AspNetCore.Authorization;

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
        [Route("Login")]
        public ActionResult<LoginDto> Login(LoginDto login)
        {
            var usuarioEncontrado =  _businessLayer.GetUserByEmail(login.Email);

            var encryptedPassword = _login.EncryptSHA256(login.Password);

            if (usuarioEncontrado == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { isSuccess = false, token = "" });
            }
            if (usuarioEncontrado.Password == encryptedPassword)
            {
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, token = _login.GenerateJWT(usuarioEncontrado) });
            }
            return StatusCode(StatusCodes.Status500InternalServerError);

        }

        [HttpPost]
        public ActionResult<User> CreateUser([FromBody] User user)
        {
            try
            {
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Invalid user data.");
                }

                var newUser = new User
                {
                    ID = user.ID,
                    Nombre = user.Nombre,
                    Edad = user.Edad,
                    Email = user.Email,
                    Deleted = user.Deleted,
                    Password = _login.EncryptSHA256(user.Password)

                };
                var createdUser = _businessLayer.CreateUser(newUser);

                if (createdUser == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, $"Could not create user {user.Nombre}.");
                }

                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.ID }, createdUser);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/Users
        [Authorize]
        [HttpGet]
        public ActionResult<List<User>> GetAllUsers()
        {
            try
            {
                var users = _businessLayer.GetAllUsers();

                if (users == null || users.Count == 0)
                {
                    return StatusCode(StatusCodes.Status404NotFound, "No users found.");
                }

                return StatusCode(StatusCodes.Status200OK, users);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
        [Authorize]
        // GET api/Users/5
        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(int id)
        {
            try
            {
                var user = _businessLayer.GetUserByID(id);

                if (user == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, $"User with ID {id} not found.");
                }

                return StatusCode(StatusCodes.Status200OK, user);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
        [Authorize]
        // GET api/Users/Email
        [HttpGet("email")]
        public ActionResult<User> GetUserByEmail(string email)
        {
            try
            {
                var user = _businessLayer.GetUserByEmail(email);

                if (user == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, $"User with Email {email} not found.");
                }

                return StatusCode(StatusCodes.Status200OK, user);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
        // PATCH api/Users
        [Authorize]
        [HttpPatch]

        //lo ideal seria hacer DTO's para req, res
        public ActionResult<User> UpdateUser([FromBody] User user)
        {
            try
            {
                var validUser = _businessLayer.GetUserByID(user.ID.Value);

                if (validUser == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Invalid user data.");
                }

                var updatedUser = _businessLayer.UpdateUser(user);

                if (updatedUser == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, $"Could not update user with ID {user.ID}.");
                }

                return StatusCode(StatusCodes.Status200OK, updatedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
        [Authorize]
        // DELETE api/Users/5
        [HttpDelete("{id}")]
        public ActionResult DeleteUser(int id)
        {
            try
            {
                bool result = _businessLayer.SoftDeleteUser(id);

                if (!result)
                {
                    return StatusCode(StatusCodes.Status404NotFound, $"User with ID {id} could not be deleted.");
                }

                return StatusCode(StatusCodes.Status200OK, $"User with ID {id} successfully soft deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}
