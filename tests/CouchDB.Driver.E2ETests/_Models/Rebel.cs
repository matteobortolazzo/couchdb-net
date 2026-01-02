namespace CouchDB.Driver.E2ETests.Models;

public record Rebel(
    string Id,
    string Name,
    string Surname,
    int Age,
    string[] Skills)
{
     public string Rev { get; init; } = null!;
}