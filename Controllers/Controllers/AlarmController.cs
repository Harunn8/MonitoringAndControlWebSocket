using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Threading.Tasks;
using Services.AlarmService;
using Services.AlarmService.Services;

namespace Controllers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlarmController : ControllerBase
    {
        private readonly AlarmManagerService _alarmManager;

        public AlarmController(AlarmManagerService alarmManager)
        {
            _alarmManager = alarmManager;
        }

        [HttpGet("GetlAllAlarms")]
        public async Task<IActionResult> GetAllAlarms()
        {
            var alarms = await _alarmManager.GetAllAlarms();

            return Ok(alarms);
        }

        [HttpGet("GetAlarmsById")]
        public async Task<IActionResult> GetAlarmsById(string id)
        {
            var entity = await _alarmManager.GetAlarmById(id);

            if (entity == null)
            {
                return BadRequest("Alarm Not Found");
            }

            return Ok(entity);
        }

        [HttpGet("GetAlarmsByDeviceId")]
        public async Task<IActionResult> GetAlarmByDeviceId(string deviceId)
        {
            var deviceAlarm = _alarmManager.GetAlarmByDeviceId(deviceId);

            if (deviceAlarm == null)
            {
                return BadRequest("Alarm Not Found");
            }

            return Ok(deviceAlarm);
        }

        [HttpPost("AddAlarm")]
        public async Task<IActionResult> CreateAlarm(AlarmModel alarmModel)
        {
            var alarm = _alarmManager.CreateAlarm(alarmModel);

            return Ok(alarm);
        }

        [HttpPut("SetActiveAlarm")]
        public async Task<IActionResult> SetActiveAlarm(string id, bool isActive = true)
        {
            var alarm = _alarmManager.GetAlarmById(id);

            if (alarm == null || alarm.Result.IsAlarmActive == true)
            {
                return BadRequest("Alarm is already active");
            }

            await _alarmManager.SetActiveAlarm(id, isActive);

            return Ok("Alarm activited");
        }

        [HttpPut("SetFixedAlarm")]
        public async Task<IActionResult> SetPassiveAlarm(string id, bool isActive = false)
        {
            var alarm = _alarmManager.GetAlarmById(id);

            if (alarm == null || alarm.Result.IsAlarmFixed == true)
            {
                return BadRequest("Alarm is already passive");
            }

            await _alarmManager.SetActiveAlarm(id, isActive);

            return Ok("Alarm passived");
        }

        [HttpPut("UpdateAlarm")]
        public async Task<IActionResult> UpdateAlarm(string id, AlarmModel alarmModel)
        {
            var alarm = await _alarmManager.GetAlarmById(id);

            if (alarm == null)
            {
                return BadRequest("Alarm Not Defined");
            }

            await _alarmManager.UpdateAlarm(id, alarmModel);

            return Ok("Alarm updated");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlarm(string id)
        {
            var alarm = await _alarmManager.GetAlarmById(id);

            if (alarm == null)
            {
                return BadRequest("Alarm Not Defined");
            }

            await _alarmManager.DeleteAlarm(id);
            return Ok("Alarm deleted");
        }

    }
}
