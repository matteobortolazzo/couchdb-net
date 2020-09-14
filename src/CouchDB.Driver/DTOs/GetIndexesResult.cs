#nullable disable
using System.Collections.Generic;
using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace CouchDB.Driver.DTOs
{
    internal class GetIndexesResult
    {
        [JsonProperty("indexes")]
        public List<IndexInfo> Indexes { get; set; }
    }
}
#nullable restore