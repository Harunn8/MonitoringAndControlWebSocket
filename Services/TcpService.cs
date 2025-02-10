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
using MQTTnet.Protocol;

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

        public async Task StartCommunicationAsync(string ipAddress, int port, string tcpFormat, Action<Dictionary<string, string>> onDataReceived, CancellationToken cancellationToken)
        {
            try
            {
                var device = await GetTcpDeviceByIpAddressAndPort(ipAddress, port);

                Log.Information($"Connecting {device.DeviceName}");
                _mqttProducer.PublishMessage("telemetry", $"Connecting to {device.DeviceName}", MqttQualityOfServiceLevel.AtMostOnce);

                _isRunning = true;
                while (!cancellationToken.IsCancellationRequested && _isRunning)
                {
                    using var client = new TcpClient();
                    await client.ConnectAsync(ipAddress, port);

                    using var stream = client.GetStream();

                    var message = Encoding.UTF8.GetBytes(tcpFormat);
                    await stream.WriteAsync(message, 0, message.Length, cancellationToken);

                    var buffer = new byte[1024];
                    var byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (byteCount > 0)
                    {
                        var data = Encoding.UTF8.GetString(buffer, 0, byteCount);
                        Log.Information($"Raw Data: {data}");

                        Dictionary<string, string> parsedData = ParseTcpData(data, device.TcpData);

                        Log.Information($"Parsed Data: {string.Join(", ", parsedData.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");

                        _mqttProducer.PublishMessage("telemetry", $"Parsed Data: {string.Join(", ", parsedData.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}", MqttQualityOfServiceLevel.AtMostOnce);

                        onDataReceived?.Invoke(parsedData);
                    }
                    client.Close();
                    await Task.Delay(200);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error during TCP communication: {e.Message}");
                _mqttProducer.PublishMessage("telemetry", $"Error during TCP communication: {e.Message}", MqttQualityOfServiceLevel.AtMostOnce);
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
                    Log.Information("Communication Stopped");
                }

                client = null;
            }
            catch (Exception e)
            {
                Log.Error($"Error Stopping Communication: {e.Message}");
            }
        }

        public async Task<TcpDevice> GetTcpDeviceByIpAddressAndPort(string id, int port)
        {
            return await _tcpDevice.Find(device => device.IpAddress == id && device.Port == port).FirstOrDefaultAsync();
        }

        public static Dictionary<string, string> ParseTcpData(string rawData, List<TcpData> tcpDataList)
        {
            if (string.IsNullOrWhiteSpace(rawData) || tcpDataList == null || tcpDataList.Count == 0)
                return new Dictionary<string, string>();

            string[] parsedValues = rawData.Split(',').Select(s => s.Trim()).ToArray();

            Dictionary<string, string> result = new Dictionary<string, string>();

            for (int i = 0; i < parsedValues.Length && i < tcpDataList.Count; i++)
            {
                string parameterName = tcpDataList[i].ParameterName;
                string value = parsedValues[i];

                if (!string.IsNullOrEmpty(parameterName))
                {
                    result[parameterName] = value;
                }
            }

            return result;
        }

    }
}
