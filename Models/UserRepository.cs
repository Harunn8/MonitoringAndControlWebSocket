using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace Models
{
    public class UserRepository : IUserRepository
    {
        private readonly List<User> _users = new List<User>
        {
            new User { Id = "1", Username = "admin", PasswordHash = "hashedpassword" }
        };

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => u.Id == userId));
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => u.Username == username));
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser == null) return false;

            existingUser.PasswordHash = user.PasswordHash;
            return await Task.FromResult(true);
        }
    }
}
