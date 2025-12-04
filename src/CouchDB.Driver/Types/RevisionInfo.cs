#nullable disable
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

public class RevisionInfo
{
    [DataMember]
    [JsonPropertyName("rev")]
    public string Rev { get; internal set; }
    
    [DataMember]
    [JsonPropertyName("status")]
    public string Status { get; internal set; }
}

#nullable restore