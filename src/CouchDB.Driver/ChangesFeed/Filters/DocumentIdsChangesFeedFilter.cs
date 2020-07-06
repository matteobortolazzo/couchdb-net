using System.Collections.Generic;

namespace CouchDB.Driver.ChangesFeed.Filters
{
    internal class DocumentIdsChangesFeedFilter : ChangesFeedFilter
    {
        public IList<string> Value { get; }

        public DocumentIdsChangesFeedFilter(IList<string> value)
        {
            Value = value;
        }
    }
}