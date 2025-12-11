using System;
using System.Text.Json.Serialization;
using CouchDB.Driver.Attributes;
using CouchDB.UnitTests.Models;

namespace CouchDB.Driver.UnitTests._Models;

[DatabaseName("custom_rebels")]
public class OtherRebel : Rebel
{
    [JsonPropertyName("rebel_bith_date")] public DateTime BirthDate { get; set; }
}