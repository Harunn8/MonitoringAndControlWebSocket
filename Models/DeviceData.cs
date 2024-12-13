using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Models
{
    public class DeviceData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("deviceId")]
        public string DeviceId { get; set; }

        [BsonElement("oid")]
        public string Oid { get; set; }

        [BsonElement("Value")]
        public string Value { get; set; }

        [BsonElement("date")]
        public DateTime TimeStamp { get; set; }
    }
}