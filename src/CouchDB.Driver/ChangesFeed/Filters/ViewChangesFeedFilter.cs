namespace CouchDB.Driver.ChangesFeed.Filters;

internal class ViewChangesFeedFilter(string value) : ChangesFeedFilter
{
    public string Value { get; } = value;
}