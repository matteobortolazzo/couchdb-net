#nullable disable
using System.Collections.Generic;
using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace CouchDB.Driver.DTOs
{
    public class ChangesFeedResponse<TSource> where TSource : CouchDocument
    {
        [JsonProperty("last_seq")]
        public string LastSequence { get; set; }

        [JsonProperty("pending")]
        public int Pending { get; set; }

        [JsonProperty("results")]
        public IList<ChangesFeedResponseResult<TSource>> Results { get; internal set; }
    }

    public class ChangesFeedResponseResult<TSource> where TSource: CouchDocument
    {
        [JsonProperty("changes")]
        public IList<ChangesFeedResponseResultChange> Changes { get; internal set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("seq")]
        public string Seq { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("doc")]
        public TSource Document { get; set; }
    }

    public class ChangesFeedResponseResultChange
    {
        [JsonProperty("rev")]
        public string Rev { get; set; }
    }
}
#nullable restore