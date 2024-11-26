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
                    return BadRequest("Invalid user data.");
                }

                var createdUser = _businessLayer.CreateUser(user);

                if (createdUser == null)
                {
                    return BadRequest($"Could not create user{user.Nombre}.");
                }

                return Ok(createdUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
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
                    return NotFound("No users found.");
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
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
                    return NotFound($"User with ID {id} not found.");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // PUT api/Users
        [HttpPut("{id}")]
        public ActionResult<User> UpdateUser([FromBody] User user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest("Invalid user data.");
                }

                var updatedUser = _businessLayer.UpdateUser(user);

                if (updatedUser == null)
                {
                    return BadRequest($"Could not update user with ID {user.ID}.");
                }

                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
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
                    return NotFound($"User with ID {id} could not be deleted.");
                }

                return Ok($"User with ID {id} successfully soft deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}