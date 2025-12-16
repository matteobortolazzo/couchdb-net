using CouchDB.Driver.Types;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal sealed record GetIndexesResult(
    [property: JsonPropertyName("indexes")]
    List<IndexInfo> Indexes
);