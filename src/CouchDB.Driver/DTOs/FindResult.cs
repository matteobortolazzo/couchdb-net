using System;
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class FindResult<T>
{
    [property:JsonPropertyName("docs")]
    public required IEnumerable<T> Docs { get; init; }

    [property:JsonPropertyName("bookmark")]
    public required string Bookmark { get; init; }

    [property:JsonPropertyName("execution_stats")]
    public ExecutionStats? ExecutionStats { get; init; }

    [property:JsonPropertyName("warning")]
    public string? Warning { get; init; }
}