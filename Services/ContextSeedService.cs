using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Services
{
    public class ContextSeedService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Device> _deviceCollection;

        public ContextSeedService(IMongoDatabase database)
        {
            _userCollection = database.GetCollection<User>("User");
            _deviceCollection = database.GetCollection<Device>("Device");
        }

        public async Task UserSeedAsync()
        {
            var user = new List<User>
             {
                 new User {Id = ObjectId.GenerateNewId().ToString(), UserName = "admin", Password = "admin"},
                 new User {Id = ObjectId.GenerateNewId().ToString(), UserName = "Operator", Password = "Operator.1"}
             };

            if (await _userCollection.CountDocumentsAsync(_ => true) == 0)
            {
                await _userCollection.InsertManyAsync(user);
            }
        }

        public async Task DeviceSeedAsync()
        {
            var device = new List<Device>
             {
                 new Device
                 {
                     Id = ObjectId.GenerateNewId().ToString(),
                     DeviceName = "Acu-Limitless",
                     IpAddress = "10.0.90.230",
                     Port = 5002,
                     //OidList = new List<string> {"1.2.3.1","1.2.3.2","1.2.3.3" ,"1.2.3.4"}
                 },
                 new Device
                 {
                     Id = ObjectId.GenerateNewId().ToString(),
                     DeviceName = "NTP Server",
                     IpAddress = "10.0.90.230",
                     Port = 5003,
                     //OidList = new List<string> {"1.3.6.1.128912.1.5.7","1.3.6.1.128912.1.7.5.1.1.2","1.3.6.1.128912.1.5.5.1.7.1.5"}
                 }
             };

            if (await _deviceCollection.CountDocumentsAsync(_ => true) == 0)
            {
                await _deviceCollection.InsertManyAsync(device);
            }
        }
    }
}