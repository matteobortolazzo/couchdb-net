#nullable disable
using Newtonsoft.Json;

namespace CouchDB.Driver.DTOs
{
    internal class CreateIndexResult
    {
        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
#nullable restore