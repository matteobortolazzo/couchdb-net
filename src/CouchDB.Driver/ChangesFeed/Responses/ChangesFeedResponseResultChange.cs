#nullable disable
using Newtonsoft.Json;

namespace CouchDB.Driver.ChangesFeed.Responses
{
    public class ChangesFeedResponseResultChange
    {
        [JsonProperty("rev")]
        public string Rev { get; set; }
    }
}
#nullable restore