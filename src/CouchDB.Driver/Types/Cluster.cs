using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

/// <summary>
/// Represents cluster information.
/// </summary>
[Serializable]
public sealed class Cluster
{
    /// <summary>
    /// The number of copies of every document.
    /// </summary>
    [JsonPropertyName("n")]
    public int Replicas { get; internal set; }

    /// <summary>
    /// The number of range partitions.
    /// </summary>
    [JsonPropertyName("q")]
    public int Shards { get; internal set; }

    /// <summary>
    /// The number of consistent copies of a document that need to be read before a successful reply.
    /// </summary>
    [JsonPropertyName("r")]
    public int ReadQuorum { get; internal set; }

    /// <summary>
    /// The number of copies of a document that need to be written before a successful reply.
    /// </summary>
    [JsonPropertyName("w")]
    public int WriteQuorum { get; internal set; }        
}