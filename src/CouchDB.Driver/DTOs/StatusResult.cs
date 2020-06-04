#nullable disable
using Newtonsoft.Json;

namespace CouchDB.Driver.DTOs
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
    internal class StatusResult
    {
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
#nullable restore