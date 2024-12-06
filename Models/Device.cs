using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Models
{
    public class Device
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

        [BsonElement("oidList")]
        public List<string> OidList { get; set; }
    }
}