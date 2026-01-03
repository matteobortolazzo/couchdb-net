using CouchDB.Driver.Types;

namespace CouchDB.Driver.DTOs;

internal record ReplicationRequest(
    [property: JsonPropertyName("cancel")]
    bool Cancel,
    [property: JsonPropertyName("continuous")]
    bool Continuous,
    [property: JsonPropertyName("create_target")]
    bool CreateTarget,
    [property: JsonPropertyName("create_target_params")]
    CreateDatabaseOptions? CreateTargetParams,
    [property: JsonPropertyName("winning_revs_only")]
    bool WinningRevOnly,
    [property: JsonPropertyName("doc_ids")]
    string[]? DocIds,
    [property: JsonPropertyName("filter")]
    string? Filter,
    [property: JsonPropertyName("selector")]
    object? Selector,
    [property: JsonPropertyName("source_proxy")]
    string? SourceProxy,
    [property: JsonPropertyName("target_proxy")]
    string? TargetProxy,
    [property: JsonPropertyName("source")]
    object Source,
    [property: JsonPropertyName("target")]
    object Target);