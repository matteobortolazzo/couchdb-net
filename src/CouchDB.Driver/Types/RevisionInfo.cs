using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

[Serializable]
public class RevisionInfo
{
    [JsonPropertyName("rev")]
    public required string Rev { get; init; }
    
    [JsonPropertyName("status")]
    public required string Status { get; init; }
}