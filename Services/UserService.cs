using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Models;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null || !VerifyPassword(user.PasswordHash, currentPassword))
            {
                return false; // Kullanıcı bulunamadı veya şifre yanlış
            }

            user.PasswordHash = HashPassword(newPassword); // Yeni şifreyi hashle
            return await _userRepository.UpdateUserAsync(user);
        }

        private bool VerifyPassword(string storedPasswordHash, string inputPassword)
        {
            // Şifre doğrulama (örnek olarak hash karşılaştırma)
            return storedPasswordHash == HashPassword(inputPassword);
        }

        private string HashPassword(string password)
        {
            // Şifreyi hashlemek için bir yöntem
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}
