#nullable disable
using System.Text.Json.Serialization;

namespace CouchDB.Driver.ChangesFeed.Responses
{
    public class ChangesFeedResponseResultChange
    {
        [JsonPropertyName("rev")]
        public string Rev { get; set; }
    }
}
#nullable restore