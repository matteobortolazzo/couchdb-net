using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class AllDocsResult<T>
{
    [JsonPropertyName("total_rows")]
    public int TotalRows { get; internal set; }

    [JsonPropertyName("offset")]
    public int Offset { get; internal set; }

    [JsonPropertyName("rows")]
    public IEnumerable<AllDocsRow<T>> Rows { get; internal set; } = new List<AllDocsRow<T>>();
}

[Serializable]
internal class AllDocsRow<T>
{
    [JsonPropertyName("id")]
    public string Id { get; internal set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; internal set; } = string.Empty;

    [JsonPropertyName("value")]
    public AllDocsValue Value { get; internal set; } = new AllDocsValue();

    [JsonPropertyName("doc")]
    public T? Doc { get; internal set; }
}

[Serializable]
internal class AllDocsValue
{
    [JsonPropertyName("rev")]
    public string Rev { get; internal set; } = string.Empty;
}