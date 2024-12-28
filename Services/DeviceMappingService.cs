using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using MongoDB.Driver;

namespace Services
{
    public class DeviceMappingService
    {
        private readonly IMongoCollection<Device> _deviceCollection;

        public DeviceMappingService(IMongoDatabase database)
        {
            _deviceCollection = database.GetCollection<Device>("Device");
        }

        public List<OidMapping> GenerateMappings(Device device, Dictionary<string, string> parameterInputs)
        {
            var mappings = new List<OidMapping>();

            foreach (var oidMapping in device.OidList)
            {
                // Eğer OID, parameterInputs'ta varsa, eşleştir
                if (parameterInputs.TryGetValue(oidMapping.Oid, out var parameterName))
                {
                    mappings.Add(new OidMapping
                    {
                        Oid = oidMapping.Oid,
                        ParameterName = parameterName
                    });
                }
                else
                {
                    // Varsayılan parametre atanabilir
                    mappings.Add(new OidMapping
                    {
                        Oid = oidMapping.Oid,
                        ParameterName = "DefaultParameter"
                    });
                }
            }

            return mappings;
        }

        public async Task SaveMappingsAsync(string deviceId, List<OidMapping> mappings)
        {
            // Veritabanındaki cihazı güncelle
            var update = Builders<Device>.Update.Set(device => device.OidList, mappings);

            await _deviceCollection.UpdateOneAsync(
                device => device.Id == deviceId,
                update
            );
        }
    }
}
