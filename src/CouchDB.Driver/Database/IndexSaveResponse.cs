using Newtonsoft.Json;

namespace CouchDB.Driver.Database
{
    public class IndexSaveResponse
    {
        [JsonProperty("result")] public string? Result { get; set; }
    }
}