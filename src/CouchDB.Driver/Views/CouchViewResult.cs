#nullable disable
#pragma warning disable CA2227 // Collection properties should be read only
using System.Collections.Generic;
using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace CouchDB.Driver.Views
{
    internal class CouchViewResult<TKey, TValue, TDoc>
        where TDoc : CouchDocument
    {
        [JsonProperty("total_rows")]
        public int TotalRows { get; set; }
        
        [JsonProperty("offset")]
        public int Offset { get; set; }
        
        [JsonProperty("rows")]
        public List<CouchViewRow<TKey, TValue, TDoc>> Rows { get; set; }
    }
}
#pragma warning restore CA2227 // Collection properties should be read only
#nullable restore