using System.Collections.Generic;

namespace CouchDB.Driver.ChangesFeed.Filters;

internal class DocumentIdsChangesFeedFilter(IList<string> value) : ChangesFeedFilter
{
    public IList<string> Value { get; } = value;
}