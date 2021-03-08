#nullable disable
namespace CouchDB.Driver.Views
{
    /// <summary>
    /// Base class for a view.
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TDoc">The type of the document.</typeparam>
    public abstract class CouchView<TKey, TDoc>
    {
        /// <summary>
        /// The document ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The view key.
        /// </summary>
        public TKey Key { get; set; }

        /// <summary>
        /// The document.
        /// </summary>
        public TDoc Document { get; set; }
    }
}
#nullable restore