#nullable disable
using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace CouchDB.Driver.DTOs
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
    internal class BulkGetResultItem<TSource> where TSource : CouchDocument
    {
        [JsonProperty("ok")]
        public TSource Item { get; set; }
    }
}
#nullable restore