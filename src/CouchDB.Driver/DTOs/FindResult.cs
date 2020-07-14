#nullable disable
using CouchDB.Driver.Types;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CouchDB.Driver.DTOs
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
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
#nullable restore