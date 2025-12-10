using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

[Serializable]
public class CouchReplicationAuth
{
    [JsonPropertyName("basic")]
    public CouchReplicationBasicCredentials? BasicCredentials { get; internal init; }
}