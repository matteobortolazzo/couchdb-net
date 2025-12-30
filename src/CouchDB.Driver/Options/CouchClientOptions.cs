using System.Text.Json;

namespace CouchDB.Driver.Options;

public record CouchClientOptions
{
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }
    public bool LogOutOnDispose { get; set; } = true;
    public bool ThrowOnQueryWarning { get; set; } = true;
}

internal record InternalCouchClientOptions(
    JsonSerializerOptions JsonSerializerOptions,
    bool LogOutOnDispose,
    bool ThrowOnQueryWarning);
