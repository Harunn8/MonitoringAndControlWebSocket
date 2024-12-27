using Microsoft.AspNetCore.Mvc;
using Services;
using System.Threading.Tasks;
using Infrastructure.Services;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly LoginService _loginService;

        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] LoginRequest request)
        {
            var isValidUser = await _loginService.ValidateUser(request.Username, request.Password);

            if (isValidUser)
            {
                var token = _loginService.GenerateJwtToken(request.Username);
                return Ok(new { Token = token });
            }

            return Unauthorized("Invalid username or password");
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
