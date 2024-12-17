using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet.Client;
using Serilog;

namespace MCSMqttBus.Connection.Base
{
    public interface IMqttConnection : IDisposable
    {
        void TryConnect();

        IMqttClient GetMqttClient();
    }
}
