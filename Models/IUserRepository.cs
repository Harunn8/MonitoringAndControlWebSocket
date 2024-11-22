using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace Models
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(string userId); // Kullanıcı ID'sine göre kullanıcı getir
        Task<User> GetUserByUsernameAsync(string username); // Kullanıcı adına göre kullanıcı getir
        Task<bool> UpdateUserAsync(User user); // Kullanıcıyı güncelle
    }
}
