using Newtonsoft.Json;

namespace CouchDB.Driver.DTOs
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
    internal class OperationResult
    {
        [JsonProperty("ok")]
        public bool Ok { get; set; }
    }
}
