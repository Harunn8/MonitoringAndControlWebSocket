using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Models;
using MongoDB.Driver;

namespace Services
{
    public class DeviceMappingService
    {
        private readonly IMongoCollection<Device> _database;

        public DeviceMappingService(IMongoDatabase database)
        {
            _database = database.GetCollection<Device>("Device");
        }

        public List<(string Oid,string ParameterName)> GenerateMappings(Device device, Dictionary<string, string> parameterInputs)
        {
            var mappings = new List<(string Oid, string ParameterName)>();

            foreach(var oid in device.OidList)
            {
                if(parameterInputs.TryGetValue(oid, out var parameterName))
                {
                    mappings.Add((oid, parameterName));
                }

                else
                {
                    mappings.Add((oid, "DefaultParameter")); // Varsayılan parametre atanabilir
                }
            }

            return mappings;
        }

        public async Task SaveMappingsAsync(Device device, Dictionary<string, string> oidToParameterMap)
        {
            var devicesToSave = oidToParameterMap.Select(mapping => new Device
            {
                Id = device.Id,
                DeviceName = device.DeviceName,
                IpAddress = device.IpAddress,
                Port = device.Port,
                OidList = new List<string> { mapping.Key }
            }).ToList();

            await _database.InsertManyAsync(devicesToSave);
        }

    }
}
