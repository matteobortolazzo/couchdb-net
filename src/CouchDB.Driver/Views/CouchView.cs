using Newtonsoft.Json;

#nullable disable
namespace CouchDB.Driver.Views
{
    /// <summary>
    /// Base class for a view.
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <typeparam name="TDoc">The type of the document.</typeparam>
    public sealed class CouchView<TKey, TValue, TDoc>
    {
        /// <summary>
        /// The document ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The view key.
        /// </summary>
        [JsonProperty("key")]
        public TKey Key { get; set; }

        /// <summary>
        /// The view key.
        /// </summary>
        [JsonProperty("value")]
        public TValue Value { get; set; }

        /// <summary>
        /// The document.
        /// </summary>
        [JsonProperty("doc")]
        public TDoc Document { get; set; }
    }
}
#nullable restore