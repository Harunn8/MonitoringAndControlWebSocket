using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using MongoDB.Driver;
using Models;
using MCSMqttBus.Producer;
using Serilog;
using MongoDB.Driver;

namespace Services
{
    public class TcpService
    {
        private readonly IMongoCollection<TcpDevice> _tcpDevice;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly MqttProducer _mqttProducer;
        private bool _isRunning;

        public TcpService(IMongoDatabase database, MqttProducer mqttProducer)
        {
            _tcpDevice = database.GetCollection<TcpDevice>("TcpDevice");
            _cancellationTokenSource = new CancellationTokenSource();
            _mqttProducer = mqttProducer;
        }

        public async Task<List<TcpDevice>> GetTcpDeviceAsync()
        {
            return await _tcpDevice.Find(device => true).ToListAsync();
        }

        public async Task<TcpDevice> GetTcpDeviceById(string id)
        {
            return await _tcpDevice.Find(device => device.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddTcpDevice(TcpDevice tcpDevice)
        {
            await _tcpDevice.InsertOneAsync(tcpDevice);
        }

        public async Task UpdateTcpDevice(string id, TcpDevice updatedTcpDevice)
        {
            await _tcpDevice.ReplaceOneAsync(device => device.Id == id, updatedTcpDevice);
        }

        public async Task DeleteTcpDevice(string id)
        {
            await _tcpDevice.DeleteOneAsync(id);
        }

        public async Task<TcpDevice> GetTcpDeviceByIp(string ipAddress)
        {
            return await _tcpDevice.Find(device => device.IpAddress == ipAddress).FirstOrDefaultAsync();
        }

        public async Task StartCommunicationAsync(string ipAddress, int port, string tcpFormat, Action<string> onDataReceived, CancellationToken cancellationToken)
        {

            //TO DO : Tek bir kere sorgu yapıyor düzeltilmeli.
            try
            {
                _isRunning = true;
                using var client = new TcpClient();
                await client.ConnectAsync(ipAddress, port);

                Log.Information($"Connected to TCP Device at {ipAddress}:{port}");
                _mqttProducer.PublishMessage("telemetry", $"Connected to Tcp Device: {ipAddress}:{port}", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);

                using var stream = client.GetStream();

                var message = Encoding.UTF8.GetBytes(tcpFormat);
                await stream.WriteAsync(message, 0, message.Length, cancellationToken);

                var buffer = new byte[1024];
                while (!cancellationToken.IsCancellationRequested && stream.CanRead && _isRunning)
                {
                    var byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);

                    byte[] bufferThatFitsData = new byte[byteCount];
                    Array.Copy(buffer, 0, bufferThatFitsData, 0, byteCount);

                    if (byteCount > 0)
                    {
                        var data = Encoding.UTF8.GetString(bufferThatFitsData, 0, byteCount);
                        Log.Information($"Received data: {data}");
                        onDataReceived?.Invoke(data);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error during TCP communication: {e.Message}");
                _mqttProducer.PublishMessage("telemetry", $"Error during TCP communication: {e.Message}",MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
            }
        }

        public void StopCommunication(TcpClient client)
        {
            try
            {
                _isRunning = false;
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();

                if (client != null && client.Connected)
                {
                    client.Close();
                    Log.Information("Tcp Communication stopped");
                }

                client = null;
            }
            catch (Exception e)
            {
                Log.Error($"Error stopping Tcp communication: {e.Message}");
            }
        }

        public async Task<TcpDevice> GetTcpDeviceByIpAddressAndPort(string id, int port)
        {
            return await _tcpDevice.Find(device => device.IpAddress == id && device.Port == port).FirstOrDefaultAsync();
        }
       
    }
}
