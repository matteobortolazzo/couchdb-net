using System.Collections.Generic;
using Newtonsoft.Json;

namespace CouchDB.Client.Responses
{
    internal class DocumentsInfo
    {
        [JsonProperty("total_rows")]
        public int TotalRows { get; internal set; }
        [JsonProperty("offset")]
        public int Offset { get; internal set; }
        [JsonProperty("rows")]
        public List<DocumentInfo> Rows { get; internal set; }
    }
}
