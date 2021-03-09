#nullable disable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CouchDB.Driver.DTOs
{
    internal class IndexDefinitionInfo
    {
        [JsonProperty("fields")]
        public Dictionary<string, string>[] Fields { get; set; }
    }
}
#nullable restore