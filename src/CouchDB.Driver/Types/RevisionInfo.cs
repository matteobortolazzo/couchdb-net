#nullable disable
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types;

public class RevisionInfo
{
    [DataMember]
    [JsonProperty("rev")]
    public string Rev { get; internal set; }
    
    [DataMember]
    [JsonProperty("status")]
    public string Status { get; internal set; }
}

#nullable restore