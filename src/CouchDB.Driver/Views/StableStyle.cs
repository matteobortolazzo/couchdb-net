namespace CouchDB.Driver.Views
{
    /// <summary>
    /// Whether or not the view results should be returned from a stable set of shards.
    /// </summary>
    public class StableStyle
    {
        private readonly string _value;

        /// <summary>
        /// The view results will be returned from a stable set of shards.
        /// </summary>
        public static StableStyle True => new("true");

        /// <summary>
        /// The view results will be returned from an unstable set of shards.
        /// </summary>
        public static StableStyle False => new("false");

        private StableStyle(string value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value;
        }
    }
}