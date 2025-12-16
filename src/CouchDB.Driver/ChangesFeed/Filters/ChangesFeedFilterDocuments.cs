using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.ChangesFeed.Filters;

internal class ChangesFeedFilterDocuments(IList<string> documentIds)
{
    [property:JsonPropertyName("doc_ids")]
    public IList<string> DocumentIds { get; init; } = documentIds;
}