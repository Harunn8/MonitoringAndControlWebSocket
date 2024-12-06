using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

namespace Services
{
    public class TcpService
    {
        public async Task StartCommunicationAsync(string ipAddress, int port, string tcpFormat, Action<string> onDataReceived, CancellationToken cancellationToken)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(ipAddress, port);

                Console.WriteLine($"Connected to TCP Device at {ipAddress}:{port}");

                using var stream = client.GetStream();

                var message = Encoding.UTF8.GetBytes(tcpFormat);
                await stream.WriteAsync(message, 0, message.Length, cancellationToken);

                var buffer = new byte[1024];
                while (!cancellationToken.IsCancellationRequested)
                {
                    var byteCount = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (byteCount > 0)
                    {
                        var data = Encoding.UTF8.GetString(buffer, 0, byteCount);
                        onDataReceived?.Invoke(data);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during TCP communication: {e.Message}");
            }
        }
    }
}
