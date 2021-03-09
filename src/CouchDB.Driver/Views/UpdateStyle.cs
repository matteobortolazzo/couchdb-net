namespace CouchDB.Driver.Views
{
    /// <summary>
    /// Whether or not the view should be updated prior to responding to the user.
    /// </summary>
    public class UpdateStyle
    {
        private readonly string _value;

        /// <summary>
        /// Updates the view prior to responding to the user.
        /// </summary>
        public static UpdateStyle True => new("true");

        /// <summary>
        /// Doesn't the view update prior to responding to the user.
        /// </summary>
        public static UpdateStyle False => new("false");

        /// <summary>
        /// Updates the view lazily when responding to the user.
        /// </summary>
        public static UpdateStyle Lazy => new("lazy");

        private UpdateStyle(string value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value;
        }
    }
}