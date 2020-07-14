namespace CouchDB.Driver.ChangesFeed.Filters
{
    internal class ViewChangesFeedFilter : ChangesFeedFilter
    {
        public string Value { get; }

        public ViewChangesFeedFilter(string value)
        {
            Value = value;
        }
    }
}