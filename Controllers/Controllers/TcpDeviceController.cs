using System;
using Microsoft.AspNetCore.Mvc;
using Services;
using Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Controllers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TcpDeviceController : ControllerBase
    {
        private readonly TcpService _tcpService;

        public TcpDeviceController(TcpService tcpService)
        {
            _tcpService = tcpService;
        }

        [HttpGet("GetAllDevice")]
        public async Task<IActionResult> GetAllTcpDevices()
        {
            var devices = await _tcpService.GetTcpDeviceAsync();
            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTcpDeviceById(string id)
        {
            var device = await _tcpService.GetTcpDeviceById(id);

            if (device == null)
            {
                return NotFound("Device not found");
            }
            return Ok(device);
        }

        [HttpPost("AddTcpDevice")]
        public async Task<IActionResult> AddTcpDevice([FromBody] TcpDevice tcpDevice)
        {
            if (tcpDevice == null)
            {
                return BadRequest("Invalid device data");
            }

            await _tcpService.AddTcpDevice(tcpDevice);
            return CreatedAtAction(nameof(GetTcpDeviceById), new { id = tcpDevice.Id }, tcpDevice);
        }

        [HttpPut("UpdateTcpDevice")]
        public async Task<IActionResult> UpdateTcpDevice(string id, [FromBody] TcpDevice updatedTcpDevice)
        {
            var device = await _tcpService.GetTcpDeviceById(id);
            if (device == null)
            {
                return BadRequest("Device not found");
            }

            await _tcpService.UpdateTcpDevice(id, updatedTcpDevice);
            return Ok(device);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTcpDevice(string id)
        {
            var device = _tcpService.GetTcpDeviceById(id);
            if (device  == null)
            {
                return BadRequest("Device not found");
            }

            await _tcpService.DeleteTcpDevice(id);
            return Ok(device);
        }

        [HttpPost("StartTcpCommunication")]
        public async Task<IActionResult> StartCommunication([FromBody] string ipAddress)
        {
            var device = await _tcpService.GetTcpDeviceByIp(ipAddress);
            if (device == null)
            {
                return NotFound("Device not found");
            }

            string tcpFormat = device.TcpFormat != null ? string.Join(",", device.TcpFormat) : string.Empty;

            _tcpService.StartCommunicationAsync(device.IpAddress, device.Port, tcpFormat,
                data => Console.WriteLine($"Data received: {data}"),
                new System.Threading.CancellationToken());

            return Ok(device);
        }

        [HttpPost("StopCommunicaiton")]
        public async Task<IActionResult> StopCommunication(TcpClient client)
        {
            _tcpService.StopCommunication(client);
            return Ok("Communication stopped");
        }


    }
}
