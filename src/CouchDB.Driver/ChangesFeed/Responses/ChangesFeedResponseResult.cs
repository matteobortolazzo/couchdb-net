#nullable disable
using System;
using System.Collections.Generic;
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.ChangesFeed.Responses
{
    public class ChangesFeedResponseResult<TSource> where TSource: CouchDocument
    {
        [JsonPropertyName("seq")]
        public string Seq { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; }

        [JsonPropertyName("changes")]
        public IList<ChangesFeedResponseResultChange> Changes { get; internal set; }

        [JsonPropertyName("roleIds")]
        public IList<string> RoleIds { get; internal set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }

        [JsonPropertyName("doc", NullValueHandling = NullValueHandling.Ignore)]
        public TSource Document { get; set; }
    }
}
#nullable restore