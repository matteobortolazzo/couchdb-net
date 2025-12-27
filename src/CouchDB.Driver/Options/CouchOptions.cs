using System.Text.Json;

namespace CouchDB.Driver.Options;

public record CouchOptions
{
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }
    public bool LogOutOnDispose { get; set; } = true;
    public bool ThrowOnQueryWarning { get; set; } = true;
}