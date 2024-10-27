using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using SnmpSharpNet;

namespace Infrastructure.Services
{
    public class SnmpService : ISnmpService
    {
        private readonly int _snmpPort = 5002;
        private bool _isRunning;

        public async Task StartContinuousCommunicationAsync(
            string ipAddress,
            List<string> oidList,
            Action<string> onMessageReceived,
            CancellationToken cancellationToken)
        {
            _isRunning = true;

            // SimpleSnmp nesnesi community olmadan çalışacak şekilde ayarlandı.
           SimpleSnmp snmpClient = new SimpleSnmp(ipAddress, "public") // Varsayılan topluluk dizgesi olarak "public"
            {
                Timeout = 2000 // Yanıt süresi (2 saniye)
            };

            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // SNMP sürümünü ver1 veya ver2 olarak deneyin.
                    var result = snmpClient.Get(SnmpVersion.Ver1, oidList.ToArray());

                    if (result != null)
                    {
                        foreach (var kvp in result)
                        {
                            onMessageReceived?.Invoke($"OID {kvp.Key}: {kvp.Value}");
                        }
                    }
                    else
                    {
                        onMessageReceived?.Invoke("Simülatörden veri alınamadı. OID'leri ve simülatör ayarlarını kontrol edin.");
                        Console.WriteLine("SNMP sonucu null döndü. OID'leri veya IP adresini doğrulayın.");
                    }
                }
                catch (Exception ex)
                {
                    onMessageReceived?.Invoke($"SNMP sorgusu sırasında hata oluştu: {ex.Message}");
                    Console.WriteLine($"SNMP sorgusu sırasında hata oluştu: {ex}");
                }

                await Task.Delay(5000); // 5 saniyelik bekleme süresi
            }
        }

        public void StopContinuousCommunication()
        {
            _isRunning = false;
        }
    }
}
