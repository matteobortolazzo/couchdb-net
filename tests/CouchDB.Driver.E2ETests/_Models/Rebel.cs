using System.Text.Json.Serialization;
using CouchDB.Driver.Attributes;

namespace CouchDB.Driver.E2ETests.Models;

[DatabaseName("rebels")]
public record Rebel(
    [property: JsonPropertyName("_id")] 
    string Id,
    string Name,
    string Surname,
    int Age,
    string[] Skills)
{
    [JsonPropertyName("_rev")] public string? Rev { get; init; }
}