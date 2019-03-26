using CouchDB.Driver.UnitTests.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Driver.UnitTests.Models
{
    [JsonObject("naboo_rebels")]
    public class OtherRebel : Rebel
    {
        [JsonProperty("birth_date")]
        public DateTime BirthDate { get; set; }
    }
}
