namespace CouchDB.Driver.DTOs;

[Serializable]
internal sealed record AllDocsResult<T>(
    [property: JsonPropertyName("total_rows")]
    int TotalRows,
    [property: JsonPropertyName("offset")]
    int Offset,
    [property: JsonPropertyName("rows")]
    AllDocsRow<T>[] Rows
);

[Serializable]
internal sealed record AllDocsRow<T>(
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("key")]
    string Key,
    [property: JsonPropertyName("value")]
    AllDocsValue Value,
    [property: JsonPropertyName("doc")]
    T? Doc
);

[Serializable]
internal sealed record AllDocsValue(
    [property: JsonPropertyName("rev")]
    string Rev
);