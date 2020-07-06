#nullable disable
using System.Collections.Generic;
using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace CouchDB.Driver.ChangesFeed.Responses
{
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
}
#nullable restore