using CouchDB.Driver.Types;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CouchDB.Driver.DTOs
{
    internal class FindResult<T>
    {
        [JsonProperty("docs")]
        public IEnumerable<T> Docs { get; internal set; }
        [JsonProperty("bookmark")]
        public string Bookmark { get; internal set; }
        [JsonProperty("execution_stats")]
        public ExecutionStats ExecutionStats { get; internal set; }
    }
}
