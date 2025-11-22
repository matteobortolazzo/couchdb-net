using Newtonsoft.Json;
using System.Collections.Generic;

namespace CouchDB.Driver.DTOs
{
    internal class AllDocsResult<T>
    {
        [JsonProperty("total_rows")]
        public int TotalRows { get; internal set; }

        [JsonProperty("offset")]
        public int Offset { get; internal set; }

        [JsonProperty("rows")]
        public IEnumerable<AllDocsRow<T>> Rows { get; internal set; } = new List<AllDocsRow<T>>();
    }

    internal class AllDocsRow<T>
    {
        [JsonProperty("id")]
        public string Id { get; internal set; } = string.Empty;

        [JsonProperty("key")]
        public string Key { get; internal set; } = string.Empty;

        [JsonProperty("value")]
        public AllDocsValue Value { get; internal set; } = new AllDocsValue();

        [JsonProperty("doc")]
        public T? Doc { get; internal set; }
    }

    internal class AllDocsValue
    {
        [JsonProperty("rev")]
        public string Rev { get; internal set; } = string.Empty;
    }
}
