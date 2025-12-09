using System.Text.Json.Serialization;
using System;

namespace CouchDB.Driver.Types;

[Serializable]
public class CouchReplicationHost
{
    [JsonPropertyName("url")]
    public string? Url { get; internal set; }

    [JsonPropertyName("auth")]
    public CouchReplicationAuth? Auth { get; internal set; }


}