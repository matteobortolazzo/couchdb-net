using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CouchDB.Driver.DTOs
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    internal class StatusResult
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
    {
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
