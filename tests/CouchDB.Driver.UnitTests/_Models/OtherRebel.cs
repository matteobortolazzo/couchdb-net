using CouchDB.Driver.UnitTests.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Driver.UnitTests.Models
{
    [JsonObject("custom_rebels")]
    public class OtherRebel : Rebel
    {
        [JsonProperty("rebel_bith_date")]
        public DateTime BirthDate { get; set; }
    }
}
