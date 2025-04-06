using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;
using Models;

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
    public List<OidMapping> OidList { get; set; }

    [BsonElement("deviceType")]
    public string DeviceType { get; set; }

    [BsonElement("Alarms")]
    public AlarmModel Alarms { get; set; }
}
public class OidMapping
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ParameterId { get; set; }
    public string Oid { get; set; }
    public string ParameterName { get; set; }
}