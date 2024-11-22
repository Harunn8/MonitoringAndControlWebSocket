using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Interfaces;
using System.Linq;

namespace Controllers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly IUserService _userService;

        public SettingsController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest(new { message = "Current password and new password are required." });
            }

            // Kullanıcı kimliğini al
            var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token or user." });
            }

            var result = await _userService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

            if (result)
            {
                return Ok(new { message = "Password changed successfully." });
            }

            return BadRequest(new { message = "Current password is incorrect or password change failed." });
        }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
