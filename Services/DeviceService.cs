using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Domain.Models;
using Models;
using MongoDB.Bson;

namespace Services
{
    public class DeviceService
    {
        private readonly IMongoCollection<Device> _device;

        public DeviceService(IMongoDatabase database)
        {
            _device = database.GetCollection<Device>("Device");
        }

        public async Task<List<Device>> GetDeviceAsync()
        {
            return await _device.Find(device => true).ToListAsync();
        }

        public async Task<Device> GetDeviceById(string id)
        {
            return await _device.Find(device => device.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddDevice(Device device)
        {
            await _device.InsertOneAsync(device);
        }

        public async Task UpdateDevice(string id, Device updatedDevice)
        {
            await _device.ReplaceOneAsync(device => device.Id == id, updatedDevice);
        }

        public async Task DeleteDevice(string id)
        {
            await _device.DeleteOneAsync(id);
        }

        public async Task<Device> GetDeviceByIp(string ipAddress)
        {
            return await _device.Find(device => device.IpAddress == ipAddress).FirstOrDefaultAsync();
        }
    }
}
