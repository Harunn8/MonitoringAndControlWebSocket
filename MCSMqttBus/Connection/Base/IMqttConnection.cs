using System;
using MQTTnet.Client;

namespace MCSMqttBus.Connection.Base
{
    public interface IMqttConnection : IDisposable
    {
        void TryConnect();

        IMqttClient GetMqttClient();
    }
}
