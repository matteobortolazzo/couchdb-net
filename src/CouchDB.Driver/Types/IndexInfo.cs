#nullable disable
using System.Collections.Generic;
using System.Linq;
using CouchDB.Driver.DTOs;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represent info about the index.
    /// </summary>
    public class IndexInfo
    {
        public IndexInfo()
        {
            Fields = new Dictionary<string, IndexFieldDirection>();
        }

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

        /// <summary>
        /// The fields used in the index
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, IndexFieldDirection> Fields { get; }

        [JsonProperty("def")]
        internal IndexDefinitionInfo Definition 
        {
            set
            {
                Fields.Clear();

                foreach (Dictionary<string, string> definitions in value.Fields)
                {
                    var (name, direction) = definitions.First();
                    IndexFieldDirection fieldDirection = direction == "asc"
                        ? IndexFieldDirection.Ascending
                        : IndexFieldDirection.Descending;
                    Fields.Add(name, fieldDirection);
                }
            }
        }
    }
}
#nullable restore