using Microsoft.AspNetCore.Mvc;
using Models;
using System.Threading.Tasks;
using Services;

namespace Controllers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var user = await _userService.GetUserAsync();
            return Ok(user);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserById(id);
            return Ok(user);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("User is required");
            }

            await _userService.AddUser(user);
            return Ok(new { message = "User added successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User updatedUser)
        {
            var existingUser = await _userService.GetUserById(id);
            if (existingUser == null)
            {
                return BadRequest("User is not found");
            }

            await _userService.UpdateDevice(id, updatedUser);
            return Ok(new { message = "User updates successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var existingUser = await _userService.GetUserById(id);
            if (id == null)
            {
                return BadRequest("User is not found");
            }

            await _userService.DeleteUser(id);
            return Ok(new { message = "User deleted successfully" });
        }
    }
}