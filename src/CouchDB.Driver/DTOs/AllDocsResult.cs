using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class AllDocsResult<T>
{
    [property:JsonPropertyName("total_rows")]
    public int TotalRows { get; init; }

    [property:JsonPropertyName("offset")]
    public int Offset { get; init; }

    [property:JsonPropertyName("rows")]
    public required IEnumerable<AllDocsRow<T>> Rows { get; init; }
}

[Serializable]
internal class AllDocsRow<T>
{
    [property:JsonPropertyName("id")]
    public required string Id { get; init; }

    [property:JsonPropertyName("key")]
    public required string Key { get; init; }

    [property:JsonPropertyName("value")]
    public required AllDocsValue Value { get; init; }

    [property:JsonPropertyName("doc")]
    public T? Doc { get; init; }
}

[Serializable]
internal class AllDocsValue
{
    [property:JsonPropertyName("rev")]
    public required string Rev { get; init; }
}