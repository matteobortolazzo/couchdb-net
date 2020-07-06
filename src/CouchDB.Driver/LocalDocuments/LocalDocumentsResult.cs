using System.Collections.Generic;
using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace CouchDB.Driver.LocalDocuments
{
#nullable disable
    internal class LocalDocumentsResult
    {
        [JsonProperty("rows")]
        public IList<LocalCouchDocument> Rows { get; set; }
    }
#nullable enable
}
