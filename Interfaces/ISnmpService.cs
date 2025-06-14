﻿using System;
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

        public Task GetSnmpDeviceByIpAndPort(string ipAddress, int port);

        void StopContinuousCommunication();

        Task SendSnmpSetCommandAsync(string ipAddress, int port, string oid, string value);
    }
}