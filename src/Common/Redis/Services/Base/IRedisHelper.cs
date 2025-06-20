using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis.Services.Base
{
    public interface IRedisHelper
    {
        Task SetAsync(string key, string value, TimeSpan? expiration = null);
        Task<string> GetAsync(string key);
        Task<bool> DeleteAsync(string key);
    }
}
