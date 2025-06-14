﻿using System;
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
        private readonly IMongoCollection<TcpDevice> _tcpDevice;
        private readonly IMongoCollection<AlarmModel> _alarmModel;

        public ContextSeedService(IMongoDatabase database)
        {
            _userCollection = database.GetCollection<User>("User");
            _deviceCollection = database.GetCollection<Device>("Devices");
            _tcpDevice = database.GetCollection<TcpDevice>("Devices");
            _alarmModel = database.GetCollection<AlarmModel>("Alarms");
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

        public async Task SnmpDeviceSeedAsync()
        {
            var device = new List<Device>
             {
                 new Device
                 {
                    Id = ObjectId.GenerateNewId().ToString(),
                    DeviceName = "Acu-Snmp",
                    DeviceType = "SNMP",
                    IpAddress = "10.0.90.230",
                    Port = 5002,
                    OidList = new List<OidMapping>()
                    {
                        new OidMapping{Oid = "1.2.3.1", ParameterId = ObjectId.GenerateNewId().ToString(), ParameterName = "Acu Process Speed"},
                        new OidMapping{Oid = "1.2.3.2", ParameterId = ObjectId.GenerateNewId().ToString(), ParameterName = "Acu Nominal Status Read Speed"}
                    }
                 },
                 new Device
                 {
                     Id = ObjectId.GenerateNewId().ToString(),
                     DeviceName = "NTP Server",
                     DeviceType = "SNMP",
                     IpAddress = "10.0.90.230",
                     Port = 5003,
                     OidList = new List<OidMapping>()
                     {
                         new OidMapping{Oid = "1.3.3.1", ParameterId = ObjectId.GenerateNewId().ToString(), ParameterName = "NTP Status"},
                         new OidMapping{Oid = "1.3.3.2", ParameterId = ObjectId.GenerateNewId().ToString(), ParameterName = "NTP Fan Status"}
                     }
                 }
             };

            if (await _deviceCollection.CountDocumentsAsync(_ => true) == 0)
            {
                await _deviceCollection.InsertManyAsync(device);
            }
        }

        public async Task TcpDeviceSeedAsync()
        {
            var tcpDevice = new List<TcpDevice>
            {
                new TcpDevice
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    DeviceName = "ACU TCP",
                    DeviceType = "TCP",
                    IpAddress = "10.0.90.230",
                    Port = 5004,
                    TcpFormat = new List<string> { "R3#", "R4#" },
                    TcpData = new List<TcpData>
                    {
                        new TcpData{ParameterId = ObjectId.GenerateNewId().ToString(), Request = "R3#", ParameterName = "Request"},
                        new TcpData{ParameterId = ObjectId.GenerateNewId().ToString(), Request = "R3#", ParameterName = "Acu Time"},
                        new TcpData{ParameterId = ObjectId.GenerateNewId().ToString(), Request = "R3#", ParameterName = "Acu Mode"},
                        new TcpData{ParameterId = ObjectId.GenerateNewId().ToString(), Request = "R3#", ParameterName = "Azimuth Position"},
                    }
                }
            };

            if (await _tcpDevice.CountDocumentsAsync(_ => true) == 2)
            {
                await _tcpDevice.InsertManyAsync(tcpDevice);
            }
        }

        public async Task AlarmSeedAsync()
        {
            var device = _deviceCollection.Find(d => d.DeviceName == "Acu-Snmp").FirstOrDefault();
            var ntpDevice = _deviceCollection.Find(d => d.DeviceName == "NTP Server").FirstOrDefault();
            var acuTcp = _tcpDevice.Find(d => d.DeviceName == "ACU TCP").FirstOrDefault();

            var alarm = new List<AlarmModel>
            {
                new AlarmModel
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    AlarmName = "Overload Acu Process Speed",
                    AlarmDescription = "Overload Acu Process Speed please check device",
                    Severity = 5,
                    AlarmCreateTime = DateTime.Now,
                    DeviceId = device.Id,
                    IsAlarmActive = true,
                    IsAlarmFixed = false,
                    IsMasked = false,
                    AlarmCondition = "==",
                    AlarmThreshold = "50",
                    DeviceType = "SNMP",
                    //AlarmStatus = AlarmType.Active,
                    ParameterId = device.OidList[0].ParameterId
                },
                new AlarmModel
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    AlarmName = "Test Alarm",
                    AlarmDescription = "Test Alarm",
                    Severity = 2,
                    AlarmCreateTime = DateTime.Now,
                    DeviceId = device.Id,
                    IsAlarmActive = false,
                    IsAlarmFixed = true,
                    IsMasked = false,
                    AlarmCondition = "==",
                    AlarmThreshold = "60",
                    DeviceType = "SNMP",
                    //AlarmStatus = AlarmType.Active,
                    ParameterId = device.OidList[1].ParameterId
                },
                new AlarmModel
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    AlarmName = "NTP Server Fault Deactive Mode",
                    AlarmDescription = "Please check NTP Server device",
                    Severity = 5,
                    AlarmCreateTime = DateTime.Now,
                    DeviceId = ntpDevice.Id,
                    IsAlarmActive = true,
                    IsAlarmFixed = false,
                    IsMasked = false,
                    AlarmCondition = "!=",
                    AlarmThreshold = "99",
                    DeviceType = "SNMP",
                    //AlarmStatus = AlarmType.Active,
                    ParameterId = ntpDevice.OidList[0].ParameterId
                },
                new AlarmModel
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    AlarmName = "ACU TCP Azimuth Locked Position",
                    AlarmDescription = "Please Press Emergency Button",
                    Severity = 5,
                    AlarmCreateTime = DateTime.Now,
                    DeviceId = acuTcp.Id,
                    IsAlarmActive = true,
                    IsAlarmFixed = false,
                    IsMasked = false,
                    AlarmCondition = ">=",
                    AlarmThreshold = "45",
                    DeviceType = "TCP",
                    //AlarmStatus = AlarmType.Active,
                    ParameterId = acuTcp.TcpData[3].ParameterId
                }
            };
            if (await _alarmModel.CountDocumentsAsync(_ => true) == 0)
            {
                await _alarmModel.InsertManyAsync(alarm);
            }
        }
    }
}