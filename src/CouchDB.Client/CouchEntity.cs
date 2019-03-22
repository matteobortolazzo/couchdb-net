using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CouchDB.Client
{
    public abstract class CouchEntity
    {
        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        private string IdOther { set => Id = value; }

        [JsonProperty("_rev", NullValueHandling = NullValueHandling.Ignore)]
        public string Rev { get; set; }
        [JsonProperty("rev", NullValueHandling = NullValueHandling.Ignore)]
        private string RevOther { set => Rev = value; }
    }
}
