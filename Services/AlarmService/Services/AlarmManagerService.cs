using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
using MQTTnet.Protocol;

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

                // Eğer alarm koşulu sağlanmadıysa (active durumundan passive olmuş olabilir) ActiveAlarm içerisinde bulunan alarmı fixler ve collection içerisinden siler
                // Burada şayet herhangi bir alarm hiç koşulu sağlamadıysa historical alarm içerisine düşmesi isActive == true olarak filtrelendiği için engellenmiştir.
                //Sadece fixleme ve alarm üretme üzerine çalışan metodtur

                if(result == false)
                {
                    var updatedHistoricalAlarm = _mapper.Map<HistoricalAlarm>(alarmModel);

                    var existingHistoricalAlarm = await _historicalAlarm.FindAsync(x => x.Id == alarmModel.Id).Result.FirstOrDefaultAsync();
                    
                    if (existingHistoricalAlarm != null && existingHistoricalAlarm.IsAlarmActive == true)
                    {
                        updatedHistoricalAlarm.IsAlarmActive =  false;
                        updatedHistoricalAlarm.IsAlarmFixed = true;
                        updatedHistoricalAlarm.IsMasked = false;
                        updatedHistoricalAlarm.UpdatedDate = DateTime.UtcNow;

                        await _historicalAlarm.ReplaceOneAsync(updatedHistoricalAlarm.Id, updatedHistoricalAlarm);
                        await _activeAlarm.DeleteOneAsync(updatedHistoricalAlarm.Id);

                        _mqttProducer.PublishMessage("alarm/notify",$"{updatedHistoricalAlarm.AlarmName} \n /{updatedHistoricalAlarm.AlarmDescription} Fixed", MqttQualityOfServiceLevel.AtMostOnce);
                    }

                    else
                    {
                        updatedHistoricalAlarm.Id = BsonObjectId.GenerateNewId().ToString();
                        updatedHistoricalAlarm.IsAlarmActive = false;
                        updatedHistoricalAlarm.IsAlarmFixed = true;
                        updatedHistoricalAlarm.IsMasked = false;
                        updatedHistoricalAlarm.AlarmCreateTime = DateTime.UtcNow;
                        updatedHistoricalAlarm.DeviceType = alarmModel.DeviceType;
                        updatedHistoricalAlarm.AlarmDescription = alarmModel.AlarmDescription;
                        updatedHistoricalAlarm.AlarmCondition = alarmModel.AlarmCondition;
                        updatedHistoricalAlarm.AlarmThreshold = alarmModel.AlarmThreshold;
                        updatedHistoricalAlarm.Severity = alarmModel.Severity;
                        updatedHistoricalAlarm.FixedDate = DateTime.UtcNow;
                        updatedHistoricalAlarm.DeviceId = alarmModel.DeviceId;
                        updatedHistoricalAlarm.ParameterId = alarmModel.ParameterId;

                        await _historicalAlarm.InsertOneAsync(updatedHistoricalAlarm);
                    }
                }

                // Fixlenen alarm'ı active, alarms collection'ı üzerinden ilk kez oluşturulacak alarmı da üretecek kısımdır
                // Result true olduğunda otomatik olarak içeri girecektir
                else
                {
                    var updatedActiveAlarm = _mapper.Map<ActiveAlarms>(alarmModel);

                    var existingActiveAlarm = await _activeAlarm.FindAsync(x => x.Id == updatedActiveAlarm.Id).Result.FirstOrDefaultAsync();
                    
                    if (existingActiveAlarm != null && existingActiveAlarm.IsAlarmFixed == true)
                    {
                        updatedActiveAlarm.IsAlarmActive = false;
                        updatedActiveAlarm.IsAlarmFixed = true;
                        updatedActiveAlarm.IsMasked = false;
                        updatedActiveAlarm.UpdatedDate = DateTime.UtcNow;

                        await _activeAlarm.ReplaceOneAsync(updatedActiveAlarm.Id, updatedActiveAlarm);
                    }

                    // Daha önce alarm oluşturulmadıysa alarmı kaydet
                    else
                    {
                        updatedActiveAlarm.Id = BsonObjectId.GenerateNewId().ToString();
                        updatedActiveAlarm.IsAlarmActive = false;
                        updatedActiveAlarm.IsAlarmFixed = true;
                        updatedActiveAlarm.IsMasked = false;
                        updatedActiveAlarm.AlarmCreateTime = DateTime.UtcNow;
                        updatedActiveAlarm.DeviceType = alarmModel.DeviceType;
                        updatedActiveAlarm.AlarmDescription = alarmModel.AlarmDescription;
                        updatedActiveAlarm.AlarmCondition = alarmModel.AlarmCondition;
                        updatedActiveAlarm.AlarmThreshold = alarmModel.AlarmThreshold;
                        updatedActiveAlarm.Severity = alarmModel.Severity;
                        updatedActiveAlarm.FixedDate = DateTime.UtcNow;
                        updatedActiveAlarm.DeviceId = alarmModel.DeviceId;
                        updatedActiveAlarm.ParameterId = alarmModel.ParameterId;

                        await _activeAlarm.InsertOneAsync(updatedActiveAlarm);
                    }
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

        public async Task<AlarmResponse> GetActiveAlarmByAlarmId(string alarmId)
        {
           var entity = await _activeAlarm.FindAsync(x => x.Id == alarmId).Result.ToListAsync();
           var activeAlarm = _mapper.Map<AlarmResponse>(entity);
           return activeAlarm;
        }

        public async Task<AlarmResponse> GetActiveAlarmByDeviceId(string deviceId)
        {
            var device = await _activeAlarm.Find(d => d.DeviceId == deviceId).ToListAsync();
            var response = _mapper.Map<AlarmResponse>(device);
            return response;
        }

        public async Task<List<AlarmResponse>> GetActiveAlarmByParameterId(string parameterId)
        {
            var parameter = await _activeAlarm.FindAsync(d => d.ParameterId == parameterId).Result.ToListAsync();
            var response = _mapper.Map<List<AlarmResponse>>(parameter);
            return response;
        }

        public async Task<List<AlarmResponse>> GetAllHistoricalAlarm()
        {
            var historicalAlarm = await _historicalAlarm.FindAsync(x => x.IsAlarmFixed == true).Result.ToListAsync();
            var response = _mapper.Map<List<AlarmResponse>>(historicalAlarm);
            return response;
        }

        public async Task<List<AlarmResponse>> GetHistoricalAlarmByAlarmId(string alarmId)
        {
            var alarm = await _historicalAlarm.FindAsync(x => x.Id == alarmId).Result.ToListAsync();
            var historicalAlarm = _mapper.Map<List<AlarmResponse>>(alarm);
            return historicalAlarm;
        }

        public async Task<List<AlarmResponse>> GetHistoricalAlarmByDeviceId(string deviceId)
        {
            var device = await _historicalAlarm.FindAsync(d => d.DeviceId == deviceId).Result.ToListAsync();
            var response = _mapper.Map<List<AlarmResponse>>(device);
            return response;
        }

        public async Task<List<AlarmResponse>> GetHistoricalAlarmByParameterId(string parameterId)
        {
            var parameter = await _historicalAlarm.FindAsync(d => d.ParameterId == parameterId).Result.ToListAsync();
            var response = _mapper.Map<List<AlarmResponse>>(parameter);
            return response;
        }
    }
}
