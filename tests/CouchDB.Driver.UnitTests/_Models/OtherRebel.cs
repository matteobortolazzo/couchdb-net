using Newtonsoft.Json;
using System;

namespace CouchDB.UnitTests.Models
{
    [JsonObject("custom_rebels")]
    public class OtherRebel : Rebel
    {
        [JsonProperty("rebel_bith_date")]
        public DateTime BirthDate { get; set; }
    }
}
