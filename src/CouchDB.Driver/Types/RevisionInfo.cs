using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

[Serializable]
public class RevisionInfo
{
    [JsonPropertyName("rev")]
    public string Rev { get; internal set; }
    
    [JsonPropertyName("status")]
    public string Status { get; internal set; }
}