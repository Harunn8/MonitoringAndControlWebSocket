using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Models
{
    public class AlarmModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("AlarmName")]
        public string AlarmName { get; set; }

        [BsonElement("AlarmDescription")]
        public string AlarmDescription { get; set; }
        
        [BsonElement("Severity")]
        public int Severity { get; set; }

        [BsonElement("AlarmCreateDate")]
        public DateTime AlarmCreateTime { get; set; }

        [BsonElement("AlarmFixedDate")]
        public DateTime FixedDate { get; set; }

        [BsonElement("DeviceId")]
        public string DeviceId { get; set; }

        [BsonElement("IsActive")]
        public bool IsAlarmActive { get; set; }

        [BsonElement("ısFixed")]
        public bool IsAlarmFixed { get; set; }

        [BsonElement("IsMasked")]
        public bool IsMasked { get; set; }

        [BsonElement("AlarmCondition")]
        public string AlarmCondition { get; set; }
        
        [BsonElement("AlarmThreshold")]
        public string AlarmThreshold { get; set; }

        [BsonElement("Device Type")]
        public string DeviceType { get; set; }

        [BsonElement("AlarmStatus")]
        public AlarmType AlarmStatus { get; set; }

    }

    public enum AlarmType
    {
        Passive = 0,
        Active = 1
    }
}
