using System;
using Models;

namespace Services
{
    public class SnmpParserService
    {
        public DeviceData Parse(string data, string deviceId)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentException("Data cannot be null or empty");
            }

            if (!data.StartsWith("OID"))
            {
                throw new ArgumentException("Data is not in the expected format. Expected 'OID' prefix");
            }

            var parts = data.Substring(4).Split(':');
            if (parts.Length != 2)
            {
                throw new FormatException("Data does not contain the expected OID and value");
            }

            var oid = parts[0].Trim();
            var valueString = parts[1].Trim().Replace(",",".");

            if (!double.TryParse(valueString, out var value))
            {
                throw new FormatException("Value part is not a valid number");
            }

            return new DeviceData
            {
                DeviceId = deviceId,
                Oid = oid,
                Value = value,
                TimeStamp = DateTime.Now
            };
        }
    }
}