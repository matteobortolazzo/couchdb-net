using System.Text.Json.Serialization;

namespace CouchDB.Driver.E2ETests.Models;

public class RebelSettings
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    public bool IsActive { get; set; }
}