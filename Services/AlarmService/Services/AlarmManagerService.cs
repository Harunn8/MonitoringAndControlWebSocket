using Models;
using Services.AlarmService.Responses;
using Services.AlarmService.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using AutoMapper;
using MQTTnet;
using MCSMqttBus.Producer;
using Infrastructure.Services;
using Serilog;
using System.Globalization;

namespace Services.AlarmService.Services
{
    public class AlarmManagerService : IAlarmManagerService
    {
        private readonly IMongoCollection<AlarmModel> _database;
        private readonly IMapper _mapper;
        private readonly DeviceService _snmpDeviceService;
        private readonly TcpService _tcpDeviceService;
        private readonly MqttProducer _mqttProducer;

        public AlarmManagerService(IMongoDatabase database, IMapper mapper, DeviceService snmpDeviceService, TcpService tcpDeviceService, MqttProducer mqttProducer)
        {
            _database = database.GetCollection<AlarmModel>("Alarms");
            _mapper = mapper;
            _snmpDeviceService = snmpDeviceService;
            _tcpDeviceService = tcpDeviceService;
            _mqttProducer = mqttProducer;
        }

        public async Task<List<AlarmModel>> CheckConditions(string deviceId, string value)
        {
            var alarms = await GetAllAlarms();

            if(!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var currentVaal))
            {
                return new List<AlarmModel>();
            }

            var matchedAlarms = new List<AlarmModel>();

            foreach(var alarm in alarms.Where(a => a.DeviceId == deviceId))
            {
               if(!double.TryParse(alarm.AlarmThreshold, NumberStyles.Any, CultureInfo.InvariantCulture, out var alarmThreshold))
                {
                    continue;
                }

                bool conditionMet = alarm.AlarmCondition switch
                {
                    ">" => currentVaal > alarmThreshold,
                    "<" => currentVaal < alarmThreshold,
                    ">=" => currentVaal >= alarmThreshold,
                    "<=" => currentVaal <= alarmThreshold,
                    "==" => currentVaal == alarmThreshold,
                    "!=" => currentVaal != alarmThreshold,
                    _ => false
                };

                if(conditionMet) 
                {
                    var alarmResponse = _mapper.Map<AlarmModel>(alarm);
                    matchedAlarms.Add(alarmResponse);
                }
            }

            return matchedAlarms;
        }

        public async Task CreateAlarm(AlarmModel alarm)
        {
            await _database.InsertOneAsync(alarm);
        }

        public async Task DeleteAlarm(string id)
        {
            await _database.DeleteOneAsync(alarm => alarm.Id == id);
        }

        public async Task<bool> ExecuteAlarm(AlarmModel alarm, string currentValue)
        {
            var matchedAlarms = await CheckConditions(alarm.DeviceId, currentValue);
        }

        public async Task<AlarmResponse> GetAlarmByDeviceId(string deviceId)
        {
            var device = await _database.FindAsync(d => d.DeviceId == deviceId).Result.ToListAsync();
            return _mapper.Map<AlarmResponse>(device);
        }

        public async Task<AlarmResponse> GetAlarmById(string id)
        {
            var alarm = await _database.FindAsync(a => a.Id == id).Result.FirstOrDefaultAsync();
            if (alarm == null)
            {
                return null;
            }
            return _mapper.Map<AlarmResponse>(alarm);
        }

        public async Task<AlarmResponse> GetAlarmsBySnmpDevice(string snmpDeviceId)
        {
            var snmpDevice = await _snmpDeviceService.GetDeviceById(snmpDeviceId);
            if (snmpDevice == null)
            {
                return null;
            }
            var alarms = await _database.FindAsync(a => a.DeviceId == snmpDevice.Id).Result.ToListAsync();
            return _mapper.Map<AlarmResponse>(alarms);
        }

        public async Task<AlarmResponse> GetAlarmsByTcpDevice(string tcpDeviceId)
        {
            var tcpDevice = _tcpDeviceService.GetTcpDeviceById(tcpDeviceId);
            if (tcpDevice == null)
            {
                return null;
            }
            var alarms = await _database.FindAsync(a => a.DeviceId == tcpDevice.Result.Id).Result.ToListAsync();
            return _mapper.Map<AlarmResponse>(alarms);
        }

        public async Task<List<AlarmResponse>> GetAllAlarms()
        {
            var alarms = await _database.FindAsync(_ => true).Result.ToListAsync();
            return _mapper.Map<List<AlarmResponse>>(alarms);
        }

        public Task<bool> SetActiveAlarm(string id, bool isActive)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetFixedAlarm(string id, bool isFixed)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAlarm(string id, AlarmModel alarm)
        {
            await _database.ReplaceOneAsync(a => a.Id == id, alarm);
        }
    }
}
