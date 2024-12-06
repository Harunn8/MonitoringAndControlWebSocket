using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Services;

namespace Controllers.Controllers
{
    public class WebSocketHandlerTcp
    {
        private readonly TcpService _tcpService;
        private readonly DeviceService _deviceService;
        private readonly DeviceDataService _deviceDataService;
        private readonly CancellationToken _cancellationToken;

        public WebSocketHandlerTcp(TcpService tcpService,DeviceService deviceService,DeviceDataService deviceDataService,CancellationToken cancellationToken)
        {
            _tcpService = tcpService;
            _deviceService = deviceService;
            _deviceDataService = deviceDataService;
            _cancellationToken = cancellationToken;
        }

        public async Task HandleAsync(HttpContext context, WebSocket webSocket)
        {
            if (webSocket == null)
            {
                throw new ArgumentException(nameof(webSocket));
            }

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            try
            {
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var command = JsonConvert.DeserializeObject<WebSocketCommand>(message);

                    switch (command.Action.ToLower())
                    {
                        case "starttcp":
                            _cancellationToken = new CancellationToken();
                            await StartTcpCommunication(command.Parameters, webSocket, _cancellationToken.Token);
                            break;
                        
                        case "stoptcp":
                            StopTcpCommunication();
                            await SendMessage(webSocket, "TCP Communication Stoped");
                            break;

                        default:
                            await SendMessage(webSocket, $"Unknown Command: {command.Action}");
                            break;
                    }
                } 
                while (!result.CloseStatus.HasValue);

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in Web Socket Communication: {e.Message}");
            }
        }

        private async Task
    }
}
