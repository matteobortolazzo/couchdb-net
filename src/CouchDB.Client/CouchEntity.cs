
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CouchDB.Client
{
    public abstract class CouchEntity
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("_rev")]
        public string Rev { get; set; }
    }
}
