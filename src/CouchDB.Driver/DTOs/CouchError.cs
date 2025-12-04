namespace CouchDB.Driver.DTOs;

internal class CouchError
{
    public required string Error { get; set; }
    public required string Reason { get; set; }
}