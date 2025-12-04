using System;
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class FindResult<T>
{
    [JsonPropertyName("docs")]
    public IEnumerable<T> Docs { get; internal set; }

    [JsonPropertyName("bookmark")]
    public string Bookmark { get; internal set; }

    [JsonPropertyName("execution_stats")]
    public ExecutionStats ExecutionStats { get; internal set; }

    [JsonPropertyName("warning")]
    public string Warning { get; internal set; }
}