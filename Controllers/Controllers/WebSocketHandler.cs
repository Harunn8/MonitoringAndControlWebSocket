using System.Collections.Generic;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Presentation.Controllers
{
    public class WebSocketHandler
    {
        private readonly ISnmpService _snmpService;
        private CancellationTokenSource _cancellationTokenSource;

        public WebSocketHandler(ISnmpService snmpService)
        {
            _snmpService = snmpService;
        }

        public async Task HandleAsync(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;
            
            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                var command = JsonConvert.DeserializeObject<WebSocketCommand>(message);


                switch (command.Action)
                {
                    case "startCommunication":
                        _cancellationTokenSource = new CancellationTokenSource();
                        StartCommunication(command.Parameters, webSocket, _cancellationTokenSource.Token);
                        break;
                    case "stopCommunication":
                        StopCommunication();
                        break;
                    default:
                        await SendMessage(webSocket, "Unknown command");
                        break;
                }

            } while (!result.CloseStatus.HasValue);

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private void StartCommunication(Dictionary<string, string> parameters, WebSocket webSocket, CancellationToken cancellationToken)
        {
            if (parameters.TryGetValue("ipAddress", out var ipAddress))
            {
              
                //var oidList = new List<string> { ".1.3.6.1.4.1.18837.3.3.2.4.0",
                //                                 ".1.3.6.1.4.1.18837.3.3.2.5.0",
                //                                 ".1.3.6.1.4.1.18837.3.3.2.6.0",
                //                                 ".1.3.6.1.4.1.18837.3.3.2.7.0",
                //                                 ".1.3.6.1.4.1.18837.3.3.2.2.0"
                //                                };

                var oidList = new List<string> { ".1.3.6.1.4.1.49034.1.8.2.2.4.0" };

                _snmpService.StartContinuousCommunicationAsync(ipAddress, oidList, async (data) =>
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                       
                        await SendMessage(webSocket, data);
                    }
                }, cancellationToken);
            }
            else
            {
                SendMessage(webSocket, "IP address is missing");
            }
        }


        private void StopCommunication()
        {
            _cancellationTokenSource?.Cancel();
            _snmpService.StopContinuousCommunication();
        }

        private async Task SendMessage(WebSocket webSocket, string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
