using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Services;
using Models;
using Services.AlarmService.Responses;
using Services.AlarmService.Services.Base;
using MCSMqttBus.Producer;
using Services;
using Serilog;
using MongoDB.Driver;
using AutoMapper;

namespace Services.AlarmService.Services
{
    public class AlarmManagerService : IAlarmManagerService
    {
        private readonly MqttProducer _mqttProducer;
        private readonly IMongoCollection<AlarmModel> _alarm;
        private readonly IMapper _mapper;
        

        public AlarmManagerService(MqttProducer mqttProducer, IMongoDatabase database, IMapper mapper)
        {
            _mqttProducer = mqttProducer;
            _alarm = database.GetCollection<AlarmModel>("Alarms");
            _mapper = mapper;
        }

        public bool CheckConditions(string parameterValue, string condition, string threshold)
        {
            var currentValue = double.TryParse(parameterValue.Trim(), out var a);
            var currentThreshold = double.TryParse(condition.Trim(), out var b);

            if (condition.Trim().Equals("!=")) return threshold != parameterValue.Trim();
            if (condition.Trim().Equals("==")) return threshold == parameterValue.Trim();
            if (!currentValue || !currentThreshold) return false;

            return condition switch
            {
                ">=" => a >= b,
                ">" => a > b,
                "<=" => a <= b,
                "<" => a < b,
                _ => false
            };

        }

        public async Task CreateAlarm(AlarmModel alarm)
        {
            await _alarm.InsertOneAsync(alarm);
        }

        public async Task DeleteAlarm(string id)
        {
            if (id == null)
            {
                return;
            }

            await _alarm.DeleteOneAsync(id);
        }

        public async Task<bool> ExecuteAlarm(AlarmModel alarmModel, string currentValue)
        {
            if (!string.IsNullOrEmpty(alarmModel.AlarmCondition) && !string.IsNullOrEmpty(alarmModel.AlarmThreshold))
            {
                var result = CheckConditions(currentValue, alarmModel.AlarmCondition, alarmModel.AlarmThreshold);
                return result;
            }

            return false;
        }

        public async Task<List<AlarmResponse>> GetAlarmByDeviceId(string deviceId)
        {
            var device =  _alarm.FindAsync(d => d.DeviceId == deviceId).Result.ToListAsync();
            var deviceResponse = _mapper.Map<List<AlarmResponse>>(device);
            return deviceResponse;
        }

        public async Task<AlarmResponse> GetAlarmById(string id)
        {
            var alarm = await _alarm.FindAsync(id).Result.FirstOrDefaultAsync();
            var alarmResponse = _mapper.Map<AlarmResponse>(alarm);
            return alarmResponse;
        }



        // Gerek olmayabilir denenecek
        public Task<AlarmResponse> GetAlarmsBySnmpDevice(string snmpDeviceId)
        {
            throw new NotImplementedException();
        }

        public Task<AlarmResponse> GetAlarmsByTcpDevice(string tcpDeviceId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<AlarmResponse>> GetAllAlarms()
        {
           var alarm = await _alarm.Find(a => true).ToListAsync();
           var alarmResponse = _mapper.Map<List<AlarmResponse>>(alarm);
           return alarmResponse;

        }

        public async Task<bool> SetActiveAlarm(string id, bool isActive)
        {
            var alarm = await GetAlarmById(id);
            if (alarm != null && alarm.IsAlarmActive == false)
            {
                alarm.IsAlarmActive = true;
                alarm.IsAlarmFixed = false;
                return true;
            }
            return false;
        }

        public async Task<bool> SetFixedAlarm(string id, bool isFixed)
        {
            var alarm = await GetAlarmById(id);
            if (alarm != null && alarm.IsAlarmFixed == false)
            {
                alarm.IsAlarmFixed = true;
                alarm.IsAlarmActive = false;
                return true;
            }
            return false;
        }

        public async Task UpdateAlarm(string id, AlarmModel alarm)
        {
            await _alarm.ReplaceOneAsync(id, alarm);
        }
    }
}
