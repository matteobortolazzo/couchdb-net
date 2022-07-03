using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents size information.
    /// </summary>
    public sealed class Sizes
    {
        /// <summary>
        /// The size of the database file on disk in bytes. Views indexes are not included in the calculation.
        /// </summary>
        [JsonProperty("file")]
        public long File { get; internal set; }

        /// <summary>
        /// The uncompressed size of database contents in bytes.
        /// </summary>
        [JsonProperty("external")]
        public long External { get; internal set; }

        /// <summary>
        /// The size of live data inside the database, in bytes.
        /// </summary>
        [JsonProperty("active")]
        public long Active { get; internal set; }
    }
}