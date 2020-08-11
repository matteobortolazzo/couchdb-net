using Newtonsoft.Json;

namespace CouchDB.Driver.Database
{
    public class IndexDesignInfo
    {
        [JsonProperty("ddoc")] public string? DDoc { get; set; }

        [JsonProperty("Name")] public string? Name { get; set; }
    }
}