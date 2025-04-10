using Services.AlarmService.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Services.AlarmService.Base
{
    public interface IAlarmRepository
    {
        public Task<List<AlarmResponse>> GetAllAlarms();
        public Task<AlarmResponse> GetAlarmById(string id);
        public Task CreateAlarm(AlarmModel alarm);
        public Task UpdateAlarm(string id, AlarmModel alarm);
        public Task DeleteAlarm(string id);
        public Task<bool> SetActiveAlarm(string id, bool isActive);
        public Task<bool> SetFixedAlarm(string id, bool isFixed);
        public Task<bool> ExecuteAlarm(AlarmModel alarm, string currentValue);
        public Task<List<AlarmResponse>> GetAlarmByDeviceId(string deviceId);
        public Task<AlarmResponse> GetAlarmsBySnmpDevice(string snmpDeviceId);
        public Task<AlarmResponse> GetAlarmsByTcpDevice(string tcpDeviceId);
        public bool CheckConditions(string parameterValue, string condition, string threshold);
    }
}
