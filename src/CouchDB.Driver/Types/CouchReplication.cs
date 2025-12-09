using System;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using CouchDB.Driver.Helpers;

namespace CouchDB.Driver.Types;

[Serializable]
[DatabaseName("_replication")]
public class CouchReplication : CouchDocument
{
    [DataMember]
    [JsonPropertyName("source")]
    public object? Source { get; internal set; }

    public CouchReplicationBasicCredentials? SourceCredentials { get; set; }

    [DataMember]
    [JsonPropertyName("target")]
    public object? Target { get; internal set; }

    public CouchReplicationBasicCredentials? TargetCredentials { get; set; }

    [DataMember]
    [JsonPropertyName("continuous")]
    public bool Continuous { get; set; }

    [DataMember]
    [JsonPropertyName("selector")]
    public object? Selector { get; set; }

    [DataMember]
    [JsonPropertyName("cancel")]
    public bool Cancel { get; internal set; }
        
    [DataMember]
    [JsonPropertyName("create_target")]
    public bool CreateTarget{ get; set; }
}