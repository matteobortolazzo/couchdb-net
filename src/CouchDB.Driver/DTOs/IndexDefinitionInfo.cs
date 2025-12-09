using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class IndexDefinitionInfo
{
    [JsonPropertyName("fields")]
    public Dictionary<string, string>[] Fields { get; set; }
}