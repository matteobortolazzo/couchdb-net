using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// The object returned from a view execution.
    /// </summary>
    /// <typeparam name="TValue">The type the value will be deserialized to.</typeparam>
    public class CouchViewRow<TValue>
    {
        /// <summary>
        /// The id of the document.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; } = null!;

        /// <summary>
        /// The view key that was emmited.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; private set; } = null!;

        /// <summary>
        /// The value that the view emmited.
        /// </summary>
        [JsonProperty("value")]
        public TValue Value { get; private set; } = default!;
    }

    /// <inheritdoc/>
    /// <typeparam name="TDoc">The type the doc will be deserialized to.</typeparam>
    public class CouchViewRow<TValue, TDoc> : CouchViewRow<TValue>
        where TDoc : CouchDocument
    {
        /// <summary>
        /// The json document deserialize to <see cref="TDoc"/>.
        /// </summary>
        [JsonProperty("doc")]
        public TDoc Doc { get; private set; } = default!;
    }
}