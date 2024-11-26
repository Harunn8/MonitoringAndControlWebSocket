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
            List<string> oidList,        // OID listesi parametresi eklendi
            Action<string> onMessageReceived,
            CancellationToken cancellationToken);

        void StopContinuousCommunication();
    }
}
