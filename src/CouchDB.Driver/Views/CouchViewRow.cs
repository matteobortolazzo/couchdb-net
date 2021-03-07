#nullable disable
using Newtonsoft.Json;

namespace CouchDB.Driver.Views
{
    public class CouchViewRow<TKey, TValue>
    {
        [JsonProperty("id")]
        public string Id { get; private set; }
        
        [JsonProperty("key")]
        public TKey Key { get; private set; }
        
        [JsonProperty("value")]
        public TValue Value { get; private set; }
    }
}
#nullable disable