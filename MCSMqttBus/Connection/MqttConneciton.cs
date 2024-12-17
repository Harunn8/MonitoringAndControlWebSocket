using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MCSMqttBus.Connection.Base;
using Serilog;
using System.Threading;

namespace MCSMqttBus.Connection
{
    public class MqttConneciton : IMqttConnection
    {
        IMqttClientOptions _mqttOptions;
        private IMqttClient _mqttClient;
        private bool _disposed;
        public MqttConneciton(IMqttClientOptions mqttOptions, IMqttClient mqttClient)
        {
            _mqttOptions = mqttOptions;
            _mqttClient = mqttClient;

            if(!_mqttClient.IsConnected && !_disposed)
            {
                TryConnect();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            try
            {
                _mqttClient.Dispose();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void TryConnect()
        {
            try
            {
                _mqttClient.UseDisconnectedHandler(e =>
                {
                    Log.Information("Disconnected from MQTT server. Reconnecting in 5 seconds...");
                    Thread.Sleep(5000);

                    _mqttClient.ReconnectAsync();
                });
                _mqttClient.ConnectAsync(_mqttOptions).Wait();
                Log.Information("Successfully connected to MQTT server");
            }
            catch (Exception ex) 
            {
                Log.Error("Failed to established MQTT connection: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public IMqttClient GetMqttClient()
        {
            if(!_mqttClient.IsConnected && !_disposed)
            {
                TryConnect();
            }
            return _mqttClient;
        }

      
    }
}
