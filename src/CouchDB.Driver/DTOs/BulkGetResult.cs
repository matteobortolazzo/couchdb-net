#nullable disable
using CouchDB.Driver.Types;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CouchDB.Driver.DTOs
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
    internal class BulkGetResult<TSource> where TSource : CouchDocument
    {
        [JsonProperty("results")]
        public List<BulkGetResultDoc<TSource>> Results { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
    internal class BulkGetResultDoc<TSource> where TSource : CouchDocument
    {
        [JsonProperty("docs")]
        public List<BulkGetResultItem<TSource>> Docs { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
    internal class BulkGetResultItem<TSource> where TSource : CouchDocument
    {
        [JsonProperty("ok")]
        public TSource Item { get; set; }
    }
}
#nullable restore