using System.Collections.Generic;
using System.Text.Json.Serialization;
using CouchDB.Driver.Attributes;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.E2ETests.Models;

[DatabaseName("rebels")]
public class Rebel : CouchDocument
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    
    [JsonPropertyName("_rev")]
    public string Rev { get; set; }
    
    public string Name { get; set; }
    public string Surname { get; set; }
    public int Age { get; set; }
    public List<string> Skills { get; set; }
}