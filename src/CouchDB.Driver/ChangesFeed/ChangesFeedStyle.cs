namespace CouchDB.Driver.ChangesFeed
{
    /// <summary>
    /// Represents the style of changes feed
    /// </summary>
    public class ChangesFeedStyle
    {
        private readonly string _value;
        /// <summary>
        /// The feed will only return the current "winning" revision;
        /// </summary>
        public static ChangesFeedStyle MainOnly => new("main_only");

        /// <summary>
        /// The feed will return all leaf revisions (including conflicts and deleted former conflicts).
        /// </summary>
        public static ChangesFeedStyle AllDocs => new("all_docs");

        private ChangesFeedStyle(string value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value;
        }
    }
}