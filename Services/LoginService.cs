using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Services
{
    public class LoginService
    {   
        private readonly UserService _userService;
        private readonly string _jwtKey;

        public LoginService(UserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _jwtKey = configuration["jwt:Key"];
        }

        public async Task<bool> ValidateUser(string userName, string password)
        {
            var user = await _userService.GetUserByUserNameAndPasswordAsync(userName, password);
            return user != null;
        }

        public string GenerateJwtToken(string userName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key =  Encoding.UTF8.GetBytes(_jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userName) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
