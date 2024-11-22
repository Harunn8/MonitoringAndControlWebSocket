using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public class User
    {
        public string Id { get; set; } // Kullanıcı ID'si
        public string Username { get; set; } // Kullanıcı adı
        public string PasswordHash { get; set; } // Hashlenmiş şifre
    }
}
