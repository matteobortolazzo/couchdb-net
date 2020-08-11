using System.Collections.Generic;
using Newtonsoft.Json;

namespace CouchDB.Driver.Database
{
    public class IndexDesignResponse
    {
        [JsonProperty("indexes")]
#pragma warning disable 8618
        public IEnumerable<IndexDesignInfo> Indexes { get; internal set; }
#pragma warning restore 8618
    }
}