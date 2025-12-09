using System;
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class FindResult<T>
{
    [JsonPropertyName("docs")]
    public IEnumerable<T> Docs { get; set; }

    [JsonPropertyName("bookmark")]
    public string Bookmark { get; set; }

    [JsonPropertyName("execution_stats")]
    public ExecutionStats ExecutionStats { get; set; }

    [JsonPropertyName("warning")]
    public string Warning { get; set; }
}