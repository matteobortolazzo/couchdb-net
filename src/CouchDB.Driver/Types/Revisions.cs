#nullable disable
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

public class Revisions
{
    [DataMember, JsonProperty("start")] public int Start { get; internal set; }

    [JsonIgnore] public IReadOnlyCollection<string> IDs { get; private set; }
    [DataMember, JsonProperty("ids")] private List<string> IdsOther { set { IDs = value?.AsReadOnly(); } }
}

#nullable restore