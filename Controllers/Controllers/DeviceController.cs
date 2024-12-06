using Microsoft.AspNetCore.Mvc;
using Models;
using System.Threading.Tasks;
using Services;

namespace Controllers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly DeviceService _deviceService;

        public DeviceController(DeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDevices()
        {
            var device = await _deviceService.GetDeviceAsync();
            return Ok(device);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeviceById(string id)
        {
            var device = await _deviceService.GetDeviceById(id);
            return Ok(device);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddDevice([FromBody] Device device)
        {
            if (device == null)
            {
                return BadRequest("Device data is required");
            }

            await _deviceService.AddDevice(device);
            return Ok(new { message = "Device added successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDevice(string id, [FromBody] Device updatedDevice)
        {
            var existingDevice = await _deviceService.GetDeviceById(id);
            if (existingDevice == null)
            {
                return NotFound("Device not found.");
            }

            await _deviceService.UpdateDevice(id, updatedDevice);
            return Ok(new { message = "Device updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(string id)
        {
            var existingDevice = await _deviceService.GetDeviceById(id);
            if (existingDevice == null)
            {
                return NotFound("Device not found.");
            }

            await _deviceService.DeleteDevice(id);
            return Ok(new { message = "Device deleted successfully!" });
        }
    }
}