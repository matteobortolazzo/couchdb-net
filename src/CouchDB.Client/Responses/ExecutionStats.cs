using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CouchDB.Client.Responses
{
    public sealed class ExecutionStats
    {
        [JsonProperty("total_keys_examined")]
        public int TotalKeysExamined { get; internal set; }
        [JsonProperty("total_docs_examined")]
        public int TotalDocsExamined { get; internal set; }
        [JsonProperty("total_quorum_docs_examined")]
        public int TotalQuorumDocsExamined { get; internal set; }
        [JsonProperty("results_returned")]
        public int ResultsReturned { get; internal set; }
        [JsonProperty("execution_time_ms")]
        public int ExecutionTimeMs { get; internal set; }
    }
}
