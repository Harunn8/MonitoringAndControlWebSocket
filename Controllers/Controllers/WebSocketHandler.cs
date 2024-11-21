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
            _snmpService = snmpService ?? throw new ArgumentNullException(nameof(snmpService));
        }

        public async Task HandleAsync(HttpContext context, WebSocket webSocket)
        {
            if (webSocket == null)
            {
                throw new ArgumentNullException(nameof(webSocket), "WebSocket cannot be null.");
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
                        break; // WebSocket kapatıldıysa çık
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        await SendMessage(webSocket, "Empty or invalid message received.");
                        continue;
                    }

                    var command = JsonConvert.DeserializeObject<WebSocketCommand>(message);
                    if (command == null || string.IsNullOrEmpty(command.Action))
                    {
                        await SendMessage(webSocket, "Invalid command format.");
                        continue;
                    }

                    // Komutları işleme
                    switch (command.Action.ToLower())
                    {
                        case "startcommunication":
                            _cancellationTokenSource = new CancellationTokenSource();
                            StartCommunication(command.Parameters, webSocket, _cancellationTokenSource.Token);
                            break;

                        case "stopcommunication":
                            StopCommunication();
                            await SendMessage(webSocket, "Communication stopped.");
                            break;

                        default:
                            await SendMessage(webSocket, $"Unknown command: {command.Action}");
                            break;
                    }

                } while (!result.CloseStatus.HasValue);

                // WebSocket kapatılma durumu
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in WebSocket communication: {ex.Message}");
                await SendMessage(webSocket, $"An error occurred: {ex.Message}");
            }
        }

        private void StartCommunication(Dictionary<string, string> parameters, WebSocket webSocket, CancellationToken cancellationToken)
        {
            if (parameters == null || !parameters.TryGetValue("ipAddress", out var ipAddress))
            {
                SendMessage(webSocket, "IP address is missing").Wait();
                return;
            }

            var oidList = new List<string>
            {
                ".1.3.6.1.4.1.18837.3.3.2.4.0",
                ".1.3.6.1.4.1.18837.3.3.2.5.0",
                ".1.3.6.1.4.1.18837.3.3.2.6.0"
            };

            try
            {
                _snmpService.StartContinuousCommunicationAsync(ipAddress, oidList, async (data) =>
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                        await SendMessage(webSocket, data);
                    }
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting communication: {ex.Message}");
                SendMessage(webSocket, $"Error starting communication: {ex.Message}").Wait();
            }
        }

        private void StopCommunication()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                _snmpService.StopContinuousCommunication();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping communication: {ex.Message}");
            }
        }

        private async Task SendMessage(WebSocket webSocket, string message)
        {
            if (webSocket.State != WebSocketState.Open)
            {
                Console.WriteLine("WebSocket is not open. Unable to send message.");
                return;
            }

            var buffer = Encoding.UTF8.GetBytes(message);
            try
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }
    }
}
