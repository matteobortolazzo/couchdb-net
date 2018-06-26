using Newtonsoft.Json;

namespace CouchDB.Client.Responses
{
    public class DocumentInfo
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }
        [JsonProperty("key")]
        public string Key { get; internal set; }
        [JsonProperty("value")]
        internal DocumentInfoValue Value { get; set; }
        [JsonProperty("rev")]
        public string Rev => Value.Rev;
    }

    public class DocumentInfoValue
    {
        [JsonProperty("rev")]
        public string Rev { get; internal set; }
    }
}