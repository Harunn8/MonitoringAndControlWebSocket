using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISnmpService
    {
        Task StartContinuousCommunicationAsync(
            string ipAddress,
            int port,
            List<string> oidList,
            Action<string> onMessageReceived,
            CancellationToken cancellationToken);

        void StopContinuousCommunication();
    }
}