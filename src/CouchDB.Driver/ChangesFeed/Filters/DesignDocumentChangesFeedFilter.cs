

namespace CouchDB.Driver.ChangesFeed.Filters;

internal class DesignDocumentChangesFeedFilter : ChangesFeedFilter
{
    public string FilterName { get; }
    public Dictionary<string, string>? QueryParameters { get; }

    public DesignDocumentChangesFeedFilter(string filterName, Dictionary<string, string>? queryParameters = null)
    {
        if (string.IsNullOrWhiteSpace(filterName))
        {
            throw new ArgumentException("Filter name cannot be null or empty.", nameof(filterName));
        }

        FilterName = filterName;
        QueryParameters = queryParameters;
    }
}