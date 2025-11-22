using System.Collections.Generic;

namespace CouchDB.Driver.ChangesFeed.Filters
{
    internal class DesignDocumentChangesFeedFilter : ChangesFeedFilter
    {
        public string FilterName { get; }
        public Dictionary<string, string>? QueryParameters { get; }

        public DesignDocumentChangesFeedFilter(string filterName, Dictionary<string, string>? queryParameters = null)
        {
            FilterName = filterName;
            QueryParameters = queryParameters;
        }
    }
}
