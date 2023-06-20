#nullable disable
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types;

public class Revisions
{
    [DataMember]
    [JsonProperty("start")]
    public string Start { get; internal set; }
    
    [DataMember]
    [JsonProperty("ids")]
    private readonly List<string> _ids;

    [JsonIgnore] public IReadOnlyCollection<string> IDs => _ids;
}

#nullable restore