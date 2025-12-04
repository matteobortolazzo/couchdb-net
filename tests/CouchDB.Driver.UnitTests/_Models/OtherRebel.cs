using System.Text.Json.Serialization;
using System;

namespace CouchDB.UnitTests.Models
{
    [JsonObject("custom_rebels")]
    public class OtherRebel : Rebel
    {
        [JsonPropertyName("rebel_bith_date")]
        public DateTime BirthDate { get; set; }
    }
}
