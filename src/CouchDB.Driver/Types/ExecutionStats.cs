using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

/// <summary>
/// Represents basic execution statistics for a specific request.
/// </summary>
[Serializable]
public sealed class ExecutionStats
{
    /// <summary>
    /// Number of index keys examined.
    /// </summary>
    [JsonPropertyName("total_keys_examined")]
    public int TotalKeysExamined { get; internal init; }

    /// <summary>
    /// Number of documents fetched from the database/index, equivalent to using include_docs=true in a view. These may then be filtered in-memory to further narrow down the result set based on the selector.
    /// </summary>
    [JsonPropertyName("total_docs_examined")]
    public int TotalDocsExamined { get; internal init; }

    /// <summary>
    /// Number of documents fetched from the database using an out-of-band document fetch. This is only non-zero when read quorum > 1 is specified in the query parameters.
    /// </summary>
    [JsonPropertyName("total_quorum_docs_examined")]
    public int TotalQuorumDocsExamined { get; internal init; }

    /// <summary>
    /// Number of results returned from the query. Ideally this should not be significantly lower than the total documents/keys examined.
    /// </summary>
    [JsonPropertyName("results_returned")]
    public int ResultsReturned { get; internal init; }

    /// <summary>
    /// Total execution time in milliseconds as measured by the database.
    /// </summary>
    [JsonPropertyName("execution_time_ms")]
    public float ExecutionTimeMs { get; internal init; }
}