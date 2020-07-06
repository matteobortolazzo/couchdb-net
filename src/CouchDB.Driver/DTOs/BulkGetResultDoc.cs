#nullable disable
using System.Collections.Generic;
using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace CouchDB.Driver.DTOs
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
    internal class BulkGetResultDoc<TSource> where TSource : CouchDocument
    {
        [JsonProperty("docs")]
        public List<BulkGetResultItem<TSource>> Docs { get; set; }
    }
}
#nullable restore