using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents cluster information.
    /// </summary>
    public sealed class Cluster
    {
        /// <summary>
        /// The number of copies of every document.
        /// </summary>
        [JsonProperty("n")]
        public int Replicas { get; internal set; }

        /// <summary>
        /// The number of range partitions.
        /// </summary>
        [JsonProperty("q")]
        public int Shards { get; internal set; }

        /// <summary>
        /// The number of consistent copies of a document that need to be read before a successful reply.
        /// </summary>
        [JsonProperty("r")]
        public int ReadQuorum { get; internal set; }

        /// <summary>
        /// The number of copies of a document that need to be written before a successful reply.
        /// </summary>
        [JsonProperty("w")]
        public int WriteQuorum { get; internal set; }        
    }
}