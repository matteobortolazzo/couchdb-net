using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

[Serializable]
public class Revisions
{
    [JsonPropertyName("start")]
    public int Start { get; init; }

    [JsonIgnore]
    public IReadOnlyCollection<string> IDs { get; private set; } = null!;

    [JsonPropertyName("ids")]
    private List<string> IdsOther { set { IDs = value.AsReadOnly(); } }
}