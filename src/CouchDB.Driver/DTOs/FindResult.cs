using CouchDB.Driver.Converters;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.DTOs;

[Serializable]
[JsonConverter(typeof(FindResultConverterFactory))]
internal sealed record FindResult<T>(
    [property: JsonPropertyName("docs")]
    T[] Docs,
    [property: JsonPropertyName("bookmark")]
    string Bookmark,
    [property: JsonPropertyName("execution_stats")]
    ExecutionStats? ExecutionStats,
    [property: JsonPropertyName("warning")]
    string? Warning
);