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
        private readonly int _snmpPort = 5005; // SNMP port numarası
        private bool _isRunning;

        public async Task StartContinuousCommunicationAsync(
            string ipAddress,
            List<string> oidList,
            Action<string> onMessageReceived,
            CancellationToken cancellationToken)
        {
            _isRunning = true;

            // SNMP hedef ayarları
            UdpTarget target = new UdpTarget(new System.Net.IPAddress(System.Net.IPAddress.Parse(ipAddress).GetAddressBytes()), _snmpPort, 1000, 1);

            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Pdu pdu = new Pdu(PduType.Get);
                    foreach (string oid in oidList)
                    {
                        pdu.VbList.Add(oid); // İlgili OID'leri ekleyin
                    }

                    // SNMP istemcisi oluştur
                    AgentParameters agentParams = new AgentParameters(new OctetString("public")) // Community değerini burada ayarlayın
                    {
                        Version = SnmpVersion.Ver2 // SNMP sürümünü ayarlayın
                    };

                    SnmpV2Packet response = (SnmpV2Packet)target.Request(pdu, agentParams);

                    if (response != null && response.Pdu.ErrorStatus == 0)
                    {
                        foreach (Vb vb in response.Pdu.VbList)
                        {
                            onMessageReceived?.Invoke($"OID {vb.Oid}: {vb.Value}");
                        }
                    }
                    else
                    {
                        onMessageReceived?.Invoke("Simülatörden veri alınamadı. OID'leri ve simülatör ayarlarını kontrol edin.");
                        Console.WriteLine("SNMP sonucu null veya hata durumu döndü. OID'leri veya IP adresini doğrulayın.");
                    }
                }
                catch (Exception ex)
                {
                    onMessageReceived?.Invoke($"SNMP sorgusu sırasında hata oluştu: {ex.Message}");
                    Console.WriteLine($"SNMP sorgusu sırasında hata oluştu: {ex}");
                }

                await Task.Delay(500); // 5 saniyelik bekleme süresi
            }

            target.Close(); // Hedefi kapat
        }

        public void StopContinuousCommunication()
        {
            _isRunning = false;
        }
    }
}
