using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Models;

namespace Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _user;

        public UserService(IMongoDatabase database)
        {
            _user = database.GetCollection<User>("User");
        }

        public async Task<List<User>> GetUserAsync()
        {
            return await _user.Find(user => true).ToListAsync();
        }

        public async Task<User> GetUserById(string id)
        {
            return await _user.Find(user => user.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddUser(User user)
        {
            await _user.InsertOneAsync(user);
        }

        public async Task UpdateDevice(string id, User updatedUser)
        {
            await _user.ReplaceOneAsync(user => user.Id == id, updatedUser);
        }

        public async Task DeleteUser(string id)
        {
            await _user.DeleteOneAsync(id);
        }

        public async Task<User> GetUserByUserNameAndPasswordAsync(string userName, string password)
        {
            return await _user
                .Find(user => user.UserName == userName && user.Password == password)
                .FirstOrDefaultAsync();
        }
    }
}