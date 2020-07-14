using System.Collections.Generic;
using Newtonsoft.Json;

namespace CouchDB.Driver.ChangesFeed.Filters
{
    internal class ChangesFeedFilterDocuments
    {
        public ChangesFeedFilterDocuments(IList<string> documentIds)
        {
            DocumentIds = documentIds;
        }

        [JsonProperty("doc_ids")]
        public IList<string> DocumentIds { get; set; }
    }
}