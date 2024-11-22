using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(string userId); // Kullanıcı ID'sine göre kullanıcı getir
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword); // Şifre değiştir
    }
}
