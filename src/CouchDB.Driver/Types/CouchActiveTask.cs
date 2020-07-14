#nullable disable
using System;
using CouchDB.Driver.Helpers;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents an active task.
    /// </summary>
    public sealed class CouchActiveTask
    {
        /// <summary>
        /// Processes changes.
        /// </summary>
        [JsonProperty("changes-done")]
        public int ChangesDone { get; internal set; }

        /// <summary>
        /// Source database.
        /// </summary>
        [JsonProperty("database")]
        public string Database { get; internal set; }

        /// <summary>
        /// Process ID
        /// </summary>
        [JsonProperty("pid")]
        public string PID { get; internal set; }

        /// <summary>
        /// Current percentage progress.
        /// </summary>
        [JsonProperty("progress")]
        public int Progress { get; internal set; }

        /// <summary>
        /// Task start time.
        /// </summary>
        [JsonProperty("started_on")]
        [JsonConverter(typeof(MicrosecondEpochConverter))]
        public DateTime StartedOn { get; internal set; }

        /// <summary>
        /// Task status message.
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; internal set; }

        /// <summary>
        /// Task name.
        /// </summary>
        [JsonProperty("task")]
        public string Task { get; internal set; }

        /// <summary>
        /// Total changes to process.
        /// </summary>
        [JsonProperty("total_changes-done")]
        public int TotalChanges { get; internal set; }

        /// <summary>
        /// Operation type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; internal set; }

        /// <summary>
        /// Last operation update.
        /// </summary>
        [JsonProperty("updated_on")]
        [JsonConverter(typeof(MicrosecondEpochConverter))]
        public DateTime UpdatedOn { get; internal set; }
    }
}
#nullable restore