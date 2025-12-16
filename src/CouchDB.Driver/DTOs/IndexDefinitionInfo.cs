using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class IndexDefinitionInfo
{
    [property:JsonPropertyName("fields")]
    public required Dictionary<string, string>[] Fields { get; init; }
}