using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class AllDocsResult<T>
{
    [JsonPropertyName("total_rows")]
    public int TotalRows { get; init; }

    [JsonPropertyName("offset")]
    public int Offset { get; init; }

    [JsonPropertyName("rows")]
    public required IEnumerable<AllDocsRow<T>> Rows { get; init; }
}

[Serializable]
internal class AllDocsRow<T>
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("value")]
    public required AllDocsValue Value { get; init; }

    [JsonPropertyName("doc")]
    public required T Doc { get; init; }
}

[Serializable]
internal class AllDocsValue
{
    [JsonPropertyName("rev")]
    public required string Rev { get; init; }
}