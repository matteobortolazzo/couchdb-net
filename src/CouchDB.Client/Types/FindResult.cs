using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Client.Types
{
    internal class FindResult<T>
    {
        [JsonProperty("docs")]
        public IEnumerable<T> Docs { get; set; }
    }
}
