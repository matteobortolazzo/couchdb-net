using System;
using CouchDB.Driver.Helpers;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    public class CouchActiveTask
    {
        [JsonProperty("changes-done")]
        public int ChangesDone { get; internal set; }
        [JsonProperty("database")]
        public string Database { get; internal set; }
        [JsonProperty("pid")]
        public string PID { get; internal set; }
        [JsonProperty("progress")]
        public int Progress { get; internal set; }
        [JsonProperty("started_on")]
        [JsonConverter(typeof(MicrosecondEpochConverter))]
        public DateTime StartedOn { get; internal set; }
        [JsonProperty("total_changes-done")]
        public int TotalChanges { get; internal set; }
        [JsonProperty("changes-done")]
        public string Type { get; internal set; }
        [JsonProperty("updated_on")]
        [JsonConverter(typeof(MicrosecondEpochConverter))]
        public DateTime UpdatedOn { get; internal set; }
    }
}
