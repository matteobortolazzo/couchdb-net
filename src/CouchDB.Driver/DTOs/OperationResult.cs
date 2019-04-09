using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CouchDB.Driver.DTOs
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    internal class OperationResult
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
    {
        [JsonProperty("ok")]
        public bool Ok { get; set; }
    }
}
