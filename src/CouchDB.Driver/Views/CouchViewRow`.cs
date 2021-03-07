#nullable disable
using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace CouchDB.Driver.Views
{
    public class CouchViewRow<TKey, TValue, TDoc> : CouchViewRow<TKey, TValue>
        where TDoc : CouchDocument
    {
        [JsonProperty("doc")]
        public TDoc Doc { get; private set; }
    }
}
#nullable restore