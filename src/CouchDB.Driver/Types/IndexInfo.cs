#nullable disable
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represent info about the index.
    /// </summary>
    public class IndexInfo
    {
        /// <summary>
        /// ID of the design document the index belongs to.
        /// </summary>
        [JsonProperty("ddoc")]
        public string DesignDocument { get; set; }

        /// <summary>
        /// The name of the index.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
#nullable restore