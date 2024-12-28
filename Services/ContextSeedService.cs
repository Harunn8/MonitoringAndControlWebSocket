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
                     OidList = new List<OidMapping>
                     {
                         new OidMapping {Oid = "1.2.3.1", ParameterName = "Acu-Process Speed"},
                         new OidMapping {Oid = "1.2.3.2", ParameterName = "Acu Nominal Status Read Speed"}
                     }
                    
                 },
                 new Device
                 {
                     Id = ObjectId.GenerateNewId().ToString(),
                     DeviceName = "NTP Server",
                     IpAddress = "10.0.90.230",
                     Port = 5003,
                     OidList = new List<OidMapping>
                     {
                         new OidMapping {Oid = "1.3.3.1", ParameterName = "NTP-Status"},
                         new OidMapping {Oid = "1.3.3.2", ParameterName = "NTP-Fan Status"}
                     }                 
                 }
             };

            if (await _deviceCollection.CountDocumentsAsync(_ => true) == 0)
            {
                await _deviceCollection.InsertManyAsync(device);
            }
        }
    }
}