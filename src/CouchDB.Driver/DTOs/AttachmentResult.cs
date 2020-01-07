using Newtonsoft.Json;

namespace CouchDB.Driver.DTOs
{
    public class AttachmentResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("ok")]
        public bool Ok { get; set; }
        [JsonProperty("rev")]
        public string Rev { get; set; }
    }
}
