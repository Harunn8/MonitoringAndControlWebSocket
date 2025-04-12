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
using Newtonsoft.Json;
using Services.AlarmService.Services;

namespace Services
{
    public class TcpService
    {
        private readonly IMongoCollection<TcpDevice> _tcpDevice;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly MqttProducer _mqttProducer;
        private readonly AlarmManagerService _alarmService;
        private readonly IMongoCollection<AlarmModel> _alarmModel;
        private bool _isRunning;

        public TcpService(IMongoDatabase database, MqttProducer mqttProducer, AlarmManagerService alarmManager, IMongoDatabase databaseTwo)
        {
            _tcpDevice = database.GetCollection<TcpDevice>("Devices");
            _cancellationTokenSource = new CancellationTokenSource();
            _mqttProducer = mqttProducer;
            _alarmService = alarmManager;
            _alarmModel = databaseTwo.GetCollection<AlarmModel>("Alarms");
        }

        public async Task<List<TcpDevice>> GetTcpDeviceAsync()
        {
            return await _tcpDevice.Find(device => device.DeviceType == "TCP").ToListAsync();
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
            await _tcpDevice.DeleteOneAsync(d=>d.Id==id);
        }

        public async Task<TcpDevice> GetTcpDeviceByIp(string ipAddress)
        {
            return await _tcpDevice.Find(device => device.IpAddress == ipAddress && device.DeviceType == "TCP").FirstOrDefaultAsync();
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

                        var  parsedData = ParseTcpData(data, device.TcpData);

                        //Log.Information($"Parsed Data: {string.Join(", ", parsedData.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");

                        _mqttProducer.PublishMessage("telemetry", $"Parsed Data: {string.Join(", ", parsedData.Select(kvp => $"{kvp.ParameterName}: {kvp.Value}"))}", MqttQualityOfServiceLevel.AtMostOnce);

                        foreach (var item in parsedData)
                        {
                            var parameterId = item.ParameterId;
                            var parameterName = item.ParameterName;
                            var value = item.Value;

                            var alarms = await _alarmModel.Find(a => a.DeviceType == "TCP" && a.DeviceId == device.Id && a.ParameterId == parameterId).ToListAsync();

                            foreach (var alarm in alarms)
                            {
                                bool isTriggered = await _alarmService.ExecuteAlarm(alarm, value);

                                var alarmId = await _alarmModel.Find(a => a.Id == alarm.Id).FirstOrDefaultAsync();

                                if(alarmId.ParameterId.Equals(alarm.ParameterId))
                                {
                                    alarm.IsAlarmActive = true;
                                    alarm.IsAlarmFixed = false;
                                    alarm.AlarmCreateTime = DateTime.Now;
                                    await _alarmService.UpdateAlarm(alarm.Id, alarm);
                                    break;
                                }

                                if (isTriggered)
                                {
                                    var newAlarm = new AlarmModel()
                                    {
                                        DeviceId = alarm.DeviceId,
                                        DeviceType = alarm.DeviceType,
                                        ParameterId = parameterId,
                                        AlarmName = alarm.AlarmName,
                                        AlarmDescription = alarm.AlarmDescription,
                                        AlarmCondition = alarm.AlarmCondition,
                                        AlarmThreshold = alarm.AlarmThreshold,
                                        Severity = alarm.Severity,
                                        IsAlarmActive = true,
                                        IsAlarmFixed = false,
                                        IsMasked = false,
                                        AlarmCreateTime = DateTime.Now
                                    };

                                    await _alarmService.CreateAlarm(newAlarm);

                                    var payload = JsonConvert.SerializeObject(newAlarm);
                                    _mqttProducer.PublishMessage("alarm/notify", $"Alarm from {device.DeviceName}/{newAlarm.AlarmDescription}/{newAlarm.AlarmCreateTime}",MqttQualityOfServiceLevel.AtMostOnce);
                                    break;
                                }
                            }

                            Console.WriteLine(value);
                        }

                        var simpleDict = parsedData.ToDictionary(x => x.ParameterName, x => x.Value);
                        onDataReceived?.Invoke(simpleDict);
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

        public static List<(string ParameterId, string ParameterName, string Value)> ParseTcpData(string rawData, List<TcpData> tcpDataList)
        {
            var result = new List<(string, string, string)>();
            if (string.IsNullOrWhiteSpace(rawData) || tcpDataList == null || tcpDataList.Count == 0)
                return result;

            string[] parsedValues = rawData.Split(',').Select(s => s.Trim()).ToArray();

            for (int i = 0; i < parsedValues.Length && i < tcpDataList.Count; i++)
            {
                result.Add((tcpDataList[i].ParameterId, tcpDataList[i].ParameterName, parsedValues[i]));
            }

            return result;
        }


    }
}
