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
using MongoDB.Bson;

namespace Services.AlarmService.Services
{
    public class AlarmManagerService : IAlarmManagerService
    {
        private readonly MqttProducer _mqttProducer;
        private readonly IMongoCollection<AlarmModel> _alarm;
        private readonly IMongoCollection<ActiveAlarms> _activeAlarm;
        private readonly IMongoCollection<HistoricalAlarm> _historicalAlarm;
        private readonly IMapper _mapper;
        

        public AlarmManagerService(MqttProducer mqttProducer, IMongoDatabase database, IMongoDatabase activeAlarm, IMongoDatabase historicalAlarm,IMapper mapper)
        {
            _mqttProducer = mqttProducer;
            _alarm = database.GetCollection<AlarmModel>("Alarms");
            _activeAlarm = database.GetCollection<ActiveAlarms>("ActiveAlarm");
            _historicalAlarm = database.GetCollection<HistoricalAlarm>("HistoricalAlarm");
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

                if(result == false)
                {
                    alarmModel.IsAlarmActive = false;
                    alarmModel.IsAlarmFixed = true;
                    await _alarm.ReplaceOneAsync(x => x.Id == alarmModel.Id, alarmModel);
                }
                else
                {
                    var existingAlarm = await _activeAlarm.Find(x => x.Id == alarmModel.Id).FirstOrDefaultAsync();

                    if (existingAlarm != null)
                    {
                        var updatedAlarm = new ActiveAlarms
                        {

                        };
                    }

                    var activeAlarm = new ActiveAlarms
                    {
                        Id = alarmModel.Id,
                        AlarmName = alarmModel.AlarmName,
                        AlarmCondition = alarmModel.AlarmCondition,
                        AlarmThreshold = alarmModel.AlarmThreshold,
                        AlarmCreateTime = DateTime.UtcNow,
                        AlarmDescription = alarmModel.AlarmDescription,
                        AlarmStatus = alarmModel.AlarmStatus,
                        IsAlarmActive = true,
                        IsAlarmFixed = false,
                        IsMasked = false,
                        Severity = alarmModel.Severity,
                        DeviceId = alarmModel.DeviceId,
                        DeviceType = alarmModel.DeviceType
                    };

                    await _activeAlarm.InsertOneAsync(activeAlarm);
                }

                return result;
            }

            return false;
        }

        public async Task<List<AlarmResponse>> GetAlarmByDeviceId(string deviceId)
        {
            var device =  await _alarm.Find(d => d.DeviceId == deviceId).ToListAsync();
            var deviceResponse = _mapper.Map<List<AlarmResponse>>(device);
            return deviceResponse;
        }

        public async Task<AlarmResponse> GetAlarmById(string id)
        {
            var alarm = await _alarm.Find(id).FirstOrDefaultAsync();
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
            var filter = Builders<AlarmModel>.Filter.Eq(x => x.Id, id);
            await _alarm.ReplaceOneAsync(filter, alarm);
        }

        public async Task<List<AlarmResponse>> GetAllActiveAlarm()
        {
            var entities = await _alarm.Find(a => a.IsAlarmActive == true).ToListAsync();
            var alarmResponse = _mapper.Map<List<AlarmResponse>>(entities);
            return alarmResponse;
        }

        public Task<List<AlarmResponse>> GetActiveAlarmByAlarmId(string alarmId)
        {
            throw new NotImplementedException();
        }

        public Task<List<AlarmResponse>> GetActiveAlarmByDeviceId(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<List<AlarmResponse>> GetActiveAlarmByParameterId(string parameterId)
        {
            throw new NotImplementedException();
        }

        public Task<List<AlarmResponse>> GetAllHistoricalAlarm()
        {
            throw new NotImplementedException();
        }

        public Task<List<AlarmResponse>> GetHistoricalAlarmByAlarmId(string alarmId)
        {
            throw new NotImplementedException();
        }

        public Task<List<AlarmResponse>> GetHistoricalAlarmByDeviceId(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<List<AlarmResponse>> GetHistoricalAlarmByParameterId(string parameterId)
        {
            throw new NotImplementedException();
        }
    }
}
