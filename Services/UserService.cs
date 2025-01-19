using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Models;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _user;
        private static readonly string key = "BuSadeceBirOrnekAnahtar12345_301";
        private static readonly string IV = "OrnekIV123456789";

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
            user.Password = Encrypt(user.Password);
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
            var user = await _user.Find(user => user.UserName == userName).FirstOrDefaultAsync();

            if(user != null)
            {
                user.Password= Decrypt(user.Password);
            }

            if(user.Password != password)
            {
                return null;
            }

            return user;
        }

        public static string Encrypt(string plainText)
        {
            byte[] keys = Encoding.UTF8.GetBytes(key);
            byte[] iv = Encoding.UTF8.GetBytes(IV);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keys;
                aes.IV = iv;

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (var writer = new StreamWriter(cryptoStream, Encoding.UTF8, 1024, leaveOpen: true))
                        {
                            writer.Write(plainText);
                        }
                        cryptoStream.FlushFinalBlock();
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }

        public static string Decrypt(string encryptedText)
        {
            byte[] keys = Encoding.UTF8.GetBytes(key);
            byte[] iv = Encoding.UTF8.GetBytes(IV);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keys;
                aes.IV = iv;

                using (var memoryStream = new MemoryStream(Convert.FromBase64String(encryptedText)))
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (var reader = new StreamReader(cryptoStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}