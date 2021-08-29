#nullable disable
using System;
using System.Collections.Generic;
using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace CouchDB.Driver.ChangesFeed.Responses
{
    public class ChangesFeedResponseResult<TSource> where TSource: CouchDocument
    {
        [JsonProperty("seq")]
        public string Seq { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("changes")]
        public IList<ChangesFeedResponseResultChange> Changes { get; internal set; }

        [JsonProperty("roleIds")]
        public IList<string> RoleIds { get; internal set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("doc", NullValueHandling = NullValueHandling.Ignore)]
        public TSource Document { get; set; }
    }
}
#nullable restore