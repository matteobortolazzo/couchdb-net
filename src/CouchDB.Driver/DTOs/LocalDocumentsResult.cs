using System.Collections.Generic;
using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace CouchDB.Driver.DTOs
{
#nullable disable
    internal class LocalDocumentsResult
    {
        [JsonProperty("rows")]
        public IList<CouchDocument> Rows { get; set; }
    }
#nullable enable
}
