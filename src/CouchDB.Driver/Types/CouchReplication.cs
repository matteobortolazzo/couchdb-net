using System;
using System.Text.Json.Serialization;
using CouchDB.Driver.Helpers;

namespace CouchDB.Driver.Types;

[Serializable]
[DatabaseName("_replication")]
public class CouchReplication : CouchDocument
{
    [JsonPropertyName("source")]
    public object? Source { get; internal set; }

    public CouchReplicationBasicCredentials? SourceCredentials { get; set; }

    [JsonPropertyName("target")]
    public object? Target { get; internal set; }

    public CouchReplicationBasicCredentials? TargetCredentials { get; set; }

    [JsonPropertyName("continuous")]
    public bool Continuous { get; set; }

    [JsonPropertyName("selector")]
    public object? Selector { get; set; }

    [JsonPropertyName("cancel")]
    public bool Cancel { get; internal set; }
        
    [JsonPropertyName("create_target")]
    public bool CreateTarget{ get; set; }
}