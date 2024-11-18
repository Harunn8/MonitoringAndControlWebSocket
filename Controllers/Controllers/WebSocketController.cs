using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using Infrastructure.Services;
using System.Collections.Generic;
using System.Threading;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebSocketController : ControllerBase
    {
        private readonly WebSocketHandler _webSocketHandler;
        private readonly SnmpService _snmpService;

        public WebSocketController(WebSocketHandler webSocketHandler)
        {
            _webSocketHandler = webSocketHandler;
        }

        [HttpGet("start")]
        public async Task<IActionResult> StartWebSocket()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                string ipAddress = "10.0.20.111";

                // İzlemek istediğiniz SNMP OID'lerini belirleyin
                List<string> oids = new List<string> { ".1.3.6.1.4.1.49034.1.8.2.2.4.0" };

                await _snmpService.StartContinuousCommunicationAsync(
                    ipAddress, oids,
                    async MessagePack =>
                    {
                        var messageBuffer = System.Text.Encoding.UTF8.GetBytes(MessagePack);
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(messageBuffer),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    },
                    CancellationToken.None

                );

                return Ok("WebSocket connection started.");
            }
            else
            {
                return BadRequest("WebSocket request expected.");
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] string message)
        {
            // Bu yöntemle WebSocket üzerinden bir mesaj gönderme işlemi yapılabilir
            // Ancak WebSocket bağlantısının aktif olması gerekiyor
            return Ok("Message sent.");
        }
    }
}