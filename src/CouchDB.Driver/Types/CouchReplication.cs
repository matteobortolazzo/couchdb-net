using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

[Serializable]
public class CouchReplication : CouchDocument
{
    [JsonPropertyName("source")]
    public object? Source { get; internal set; }

    public CouchReplicationBasicCredentials? SourceCredentials { get; init; }

    [JsonPropertyName("target")]
    public object? Target { get; internal set; }

    public CouchReplicationBasicCredentials? TargetCredentials { get; init; }

    [JsonPropertyName("continuous")]
    public bool Continuous { get; init; }

    [JsonPropertyName("selector")]
    public object? Selector { get; init; }

    [JsonPropertyName("cancel")]
    public bool Cancel { get; internal set; }
        
    [JsonPropertyName("create_target")]
    public bool CreateTarget{ get; init; }
}