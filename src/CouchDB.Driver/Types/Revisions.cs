using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

[Serializable]
public class Revisions
{
    [JsonPropertyName("start")]
    public int Start { get; internal set; }

    [JsonIgnore]
    public IReadOnlyCollection<string> IDs { get; private set; }

    [JsonPropertyName("ids")]
    private List<string> IdsOther { set { IDs = value?.AsReadOnly(); } }
}