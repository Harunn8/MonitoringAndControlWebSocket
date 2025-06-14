﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Services
{
    public class DeviceService
    {
        private readonly IMongoCollection<Device> _device;

        public DeviceService(IMongoDatabase database)
        {
            _device = database.GetCollection<Device>("Devices");
        }

        public async Task<List<Device>> GetDeviceAsync()
        {
            return await _device.Find(device => device.DeviceType == "SNMP").ToListAsync();
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

        public async Task<Device> GetDeviceByIp(string ipAddress,int port)
        {
            return await _device.Find(device => device.IpAddress == ipAddress && device.Port == port && device.DeviceType == "SNMP").FirstOrDefaultAsync();
        }
    }
}