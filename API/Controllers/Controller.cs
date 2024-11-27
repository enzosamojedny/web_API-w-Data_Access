using Microsoft.AspNetCore.Mvc;
using BLL;
using Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly BusinessLayer _businessLayer;

        public UsersController()
        {
            _businessLayer = new BusinessLayer();
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

                var createdUser = _businessLayer.CreateUser(user);

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
