using System;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;

namespace CouchDB.Driver.Types;

[Serializable]
public class CouchReplicationAuth
{
    [DataMember]
    [JsonPropertyName("basic")]
    public CouchReplicationBasicCredentials? BasicCredentials { get; internal set; }
}