using Newtonsoft.Json;
using System.Collections.Generic;

namespace CouchDB.Driver.Types
{
    internal class FindResult<T>
    {
        [JsonProperty("docs")]
        public IEnumerable<T> Docs { get; set; }
    }
}
