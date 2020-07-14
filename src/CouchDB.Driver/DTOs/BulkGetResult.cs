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
}
#nullable restore