#nullable disable
using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace CouchDB.Driver.Views
{
    internal class CouchViewRow<TKey, TValue, TDoc>
        where TDoc : CouchDocument
    {
        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("key")]
        public TKey Key { get; private set; }

        [JsonProperty("value")]
        public TValue Value { get; private set; }

        [JsonProperty("doc")]
        public TDoc Doc { get; private set; }
    }
}
#nullable restore