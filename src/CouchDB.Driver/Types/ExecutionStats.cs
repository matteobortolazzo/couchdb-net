namespace CouchDB.Driver.Types;

/// <summary>
/// Represents basic execution statistics for a specific request.
/// </summary>
/// <param name="TotalKeysExamined">Number of index keys examined.</param>
/// <param name="TotalDocsExamined">Number of documents fetched from the database/index, equivalent to using include_docs=true in a view. These may then be filtered in-memory to further narrow down the result set based on the selector.</param>
/// <param name="TotalQuorumDocsExamined">Number of documents fetched from the database using an out-of-band document fetch. This is only non-zero when read quorum > 1 is specified in the query parameters.</param>
/// <param name="ResultsReturned">Number of results returned from the query. Ideally this should not be significantly lower than the total documents/keys examined.</param>
/// <param name="ExecutionTimeMs">Total execution time in milliseconds as measured by the database.</param>
[Serializable]
public sealed record ExecutionStats(
    [property: JsonPropertyName("total_keys_examined")]
    int TotalKeysExamined,
    [property: JsonPropertyName("total_docs_examined")]
    int TotalDocsExamined,
    [property: JsonPropertyName("total_quorum_docs_examined")]
    int TotalQuorumDocsExamined,
    [property: JsonPropertyName("results_returned")]
    int ResultsReturned,
    [property: JsonPropertyName("execution_time_ms")]
    float ExecutionTimeMs
);