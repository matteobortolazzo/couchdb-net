#nullable disable
using System.Collections.Generic;
using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace CouchDB.Driver.ChangesFeed.Responses
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
}
#nullable restore