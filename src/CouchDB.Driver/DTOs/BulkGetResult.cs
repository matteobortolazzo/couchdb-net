using CouchDB.Driver.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchDB.Driver.DTOs
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    internal class BulkGetResult<TSource> where TSource : CouchDocument
    {
        [JsonProperty("results")]
        public List<BulkGetResultDoc<TSource>> Results { get; set; }
    }

    internal class BulkGetResultDoc<TSource> where TSource : CouchDocument
    {
        [JsonProperty("docs")]
        public List<BulkGetResultItem<TSource>> Docs { get; set; }
    }

    internal class BulkGetResultItem<TSource> where TSource : CouchDocument
    {
        [JsonProperty("ok")]
        public TSource Item { get; set; }
    }
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
}
