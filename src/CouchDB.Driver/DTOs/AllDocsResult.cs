using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class AllDocsResult<T>
{
    [JsonPropertyName("total_rows")]
    public int TotalRows { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("rows")]
    public IEnumerable<AllDocsRow<T>> Rows { get; set; }
}

[Serializable]
internal class AllDocsRow<T>
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("value")]
    public AllDocsValue Value { get; set; }

    [JsonPropertyName("doc")]
    public T Doc { get; set; }
}

[Serializable]
internal class AllDocsValue
{
    [JsonPropertyName("rev")]
    public string Rev { get; set; }
}