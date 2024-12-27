using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Models;

namespace Services
{
    public class DeviceDataService
    {
        private readonly IMongoCollection<DeviceData> _deviceData;

        public DeviceDataService(IMongoDatabase database)
        {
            _deviceData = database.GetCollection<DeviceData>("Device");
        }

        public async Task<List<DeviceData>> GetDeviceDataAsync()
        {
            return await _deviceData.Find(deviceData => true).ToListAsync();
        }

        public async Task<DeviceData> GetDeviceDataById(string id)
        {
            return await _deviceData.Find(deviceData => deviceData.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<DeviceData>> GetDeviceDataToDeviceId(string id)
        {
            return await _deviceData.Find(deviceData => deviceData.DeviceId == id).ToListAsync();
        }

        // Operasyon sırasında kullanılmayacak
        public async Task AddDeviceData(string deviceId, string oid, string value)
        {
            var deviceData = new DeviceData
            {
                DeviceId = deviceId,
                Oid = oid,
                Value = value,
                TimeStamp = DateTime.Now
            };
        }

        // Operasyon sırasında kullanılmayacak
        public async Task UpdateDeviceData(string id, DeviceData updatedDeviceData)
        {
            await _deviceData.ReplaceOneAsync(deviceData => deviceData.Id == id, updatedDeviceData);
        }

        // Operasyon sırasında kullanılmayacak
        public async Task DeleDeviceData(string id)
        {
            await _deviceData.DeleteOneAsync(deviceData => deviceData.Id == id);
        }

    }
}