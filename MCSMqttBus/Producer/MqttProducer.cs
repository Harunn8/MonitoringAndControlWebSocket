using System;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MCSMqttBus.Connection.Base;

namespace MCSMqttBus.Producer
{
    public class MqttProducer
    {
        private readonly IMqttConnection _connection;

        public MqttProducer(IMqttConnection mqttConnection)
        {
            _connection = mqttConnection;
        }

        public virtual bool GetMqttConnectionStatus()
        {
            return _connection.GetMqttClient().IsConnected;
        }

        public virtual void PublishMessage(string topic,string message, MqttQualityOfServiceLevel qosLevel)
        {
            var client = _connection.GetMqttClient();
            {
                var sendingMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel(qosLevel)
                    .WithPayload(message)
                    .Build();
                
                client.PublishAsync(sendingMessage).Wait();
            }
        }
    }
}
