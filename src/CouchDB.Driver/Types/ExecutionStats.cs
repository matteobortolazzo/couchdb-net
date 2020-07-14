using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents basic execution statistics for a specific request.
    /// </summary>
    public sealed class ExecutionStats
    {
        /// <summary>
        /// Number of index keys examined.
        /// </summary>
        [JsonProperty("total_keys_examined")]
        public int TotalKeysExamined { get; internal set; }

        /// <summary>
        /// Number of documents fetched from the database/index, equivalent to using include_docs=true in a view. These may then be filtered in-memory to further narrow down the result set based on the selector.
        /// </summary>
        [JsonProperty("total_docs_examined")]
        public int TotalDocsExamined { get; internal set; }

        /// <summary>
        /// Number of documents fetched from the database using an out-of-band document fetch. This is only non-zero when read quorum > 1 is specified in the query parameters.
        /// </summary>
        [JsonProperty("total_quorum_docs_examined")]
        public int TotalQuorumDocsExamined { get; internal set; }

        /// <summary>
        /// Number of results returned from the query. Ideally this should not be significantly lower than the total documents/keys examined.
        /// </summary>
        [JsonProperty("results_returned")]
        public int ResultsReturned { get; internal set; }

        /// <summary>
        /// Total execution time in milliseconds as measured by the database.
        /// </summary>
        [JsonProperty("execution_time_ms")]
        public int ExecutionTimeMs { get; internal set; }
    }
}
