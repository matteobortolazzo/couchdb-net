using System.Text.Json.Serialization;
using System;
using System.Runtime.Serialization;

namespace CouchDB.Driver.Types;

[Serializable]
public class CouchReplicationHost
{
    [DataMember]
    [JsonPropertyName("url")]
    public string? Url { get; internal set; }

    [DataMember]
    [JsonPropertyName("auth")]
    public CouchReplicationAuth? Auth { get; internal set; }


}