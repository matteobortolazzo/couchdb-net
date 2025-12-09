using System.Text.Json.Serialization;
using System;
using CouchDB.Driver.Helpers;

namespace CouchDB.UnitTests.Models
{
    [DatabaseName("custom_rebels")]
    public class OtherRebel : Rebel
    {
        [JsonPropertyName("rebel_bith_date")] public DateTime BirthDate { get; set; }
    }
}