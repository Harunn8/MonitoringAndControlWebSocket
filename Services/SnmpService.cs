using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Models;
using SnmpSharpNet;
using MCSMqttBus.Producer;
using MongoDB.Driver;
using Serilog;
using Services.AlarmService;
using Services.AlarmService.Services;

namespace Infrastructure.Services
{
    public class SnmpService : ISnmpService
    {
        private bool _isRunning;
        private readonly MqttProducer _mqttProducer;
        private readonly AlarmManagerService _alarmManager;
        private readonly IMongoCollection<AlarmModel> _alarmCollection;
        private readonly IMongoCollection<Device> _deviceCollection;

        public SnmpService(MqttProducer mqttProducer, AlarmManagerService alarmManeger, IMongoDatabase database, IMongoDatabase databaseTwo)
        {
            _mqttProducer = mqttProducer;
            _alarmManager = alarmManeger;
            _alarmCollection = database.GetCollection<AlarmModel>("Alarms");
            _deviceCollection = databaseTwo.GetCollection<Device>("Devices");
        }

        public async Task StartContinuousCommunicationAsync(
            string ipAddress,
            int port,
            List<string> oidList,
            Action<string> onMessageReceived,
            CancellationToken cancellationToken)
        {
            _isRunning = true;

            var device = await _deviceCollection.FindAsync(d => d.IpAddress == ipAddress && d.Port == port).Result.FirstOrDefaultAsync();

            UdpTarget target = new UdpTarget(new System.Net.IPAddress(System.Net.IPAddress.Parse(ipAddress).GetAddressBytes()), port, 1000, 1);

            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Pdu pdu = new Pdu(PduType.Get);
                    foreach (string oid in oidList)
                    {
                        pdu.VbList.Add(oid);
                    }

                    AgentParameters agentParams = new AgentParameters(new OctetString("public"))
                    {
                        Version = SnmpVersion.Ver2
                    };

                    SnmpV2Packet response = (SnmpV2Packet)target.Request(pdu, agentParams);

                    if (response != null && response.Pdu.ErrorStatus == 0)
                    {
                        foreach (Vb vb in response.Pdu.VbList)
                        {
                            string oid = vb.Oid.ToString();
                            string value = vb.Value.ToString().Trim();

                            var alarms = await _alarmCollection.FindAsync(a => a.DeviceType == "SNMP" && a.DeviceId == device.Id);
                            onMessageReceived?.Invoke($"OID {vb.Oid}: {vb.Value} ");
                            _mqttProducer.PublishMessage("telemetry",$"{vb.Oid},{vb.Value}",MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
                            Console.WriteLine($"{vb.Oid}: {vb.Value}");
                        }
                    }
                    else
                    {
                        onMessageReceived?.Invoke("SNMP result returned null or error status. Verify OIDs or IP address");
                        Log.Warning("SNMP result returned null or error status. Verify OIDs or IP address");
                        
                    }
                }
                catch (Exception ex)
                {
                    onMessageReceived?.Invoke($"Error occurred during SNMP query: {ex.Message}");
                    Log.Error($"Error occurred during SNMP query: {ex}");
                }

                await Task.Delay(500);
            }

            target.Close();
        }

        public void StopContinuousCommunication()
        {
            _isRunning = false;
        }

        public async Task SendSnmpSetCommandAsync(string ipAddress, int port, string oid, string value)
        {
            try
            {
                UdpTarget target = new UdpTarget(new System.Net.IPAddress(System.Net.IPAddress.Parse(ipAddress).GetAddressBytes()), port, 1000, 1);
                
                Pdu pdu = new Pdu(PduType.Set);
                pdu.VbList.Add(new Vb(new Oid(oid), new OctetString(value)));

                AgentParameters agentParameters = new AgentParameters(new OctetString("private"))
                {
                    Version = SnmpVersion.Ver2
                };

                SnmpV2Packet response = (SnmpV2Packet)target.Request(pdu, agentParameters);

                if (response != null && response.Pdu.ErrorStatus == 0)
                {
                    _mqttProducer.PublishMessage("telemetry", $"{oid},{value} command send was successfully", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
                    Log.Information($"Command was send successfully to {oid},{value}");
                }

                else
                {
                    _mqttProducer.PublishMessage("telemetry", $"Error! This command can not sended be successfully", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
                    Log.Information("Error! This command can not sended be successfully");
                }
            }
            catch(Exception ex) 
            {
                Log.Information("Error :", ex.Message);
            }
        }

        public Task GetSnmpDeviceByIpAndPort(string ipAddress, int port)
        {
            throw new NotImplementedException();
        }
    }
}