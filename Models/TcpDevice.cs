using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Models
{
    public class TcpDevice
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("deviceName")]
        public string DeviceName { get; set; }

        [BsonElement("ipAddress")]
        public string IpAddress { get; set; }

        [BsonElement("port")]
        public int Port { get; set; }

        [BsonElement("tcpFormat")]
        public List<string> TcpFormat { get; set; }

        [BsonElement("tcpData")]
        public List<TcpData> TcpData {  get; set; }

        [BsonElement("deviceType")]
        public string DeviceType { get; set; }
    }

    public class TcpData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ParameterId { get; set; }
        public string Request { get; set; }
        public string ParameterName { get; set; }
    }
}
