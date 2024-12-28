using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;

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
}
public class OidMapping
{
    public string Oid { get; set; }
    public string ParameterName { get; set; }
}