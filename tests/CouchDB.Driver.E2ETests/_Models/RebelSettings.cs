using System.Text.Json.Serialization;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.E2ETests.Models;

public class RebelSettings: CouchDocument
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    public bool IsActive { get; set; }
}