using System.Net.Http;
using System.Text.Json;

namespace CouchDB.Driver.Options;

public record CouchClientOptions
{
    public bool LogOutOnDispose { get; set; } = true;
    public bool ThrowOnQueryWarning { get; set; } = true;

    [JsonIgnore]
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    [JsonIgnore]
    public HttpClient? HttpClient { get; set; }
}

internal record InternalCouchClientOptions(
    JsonSerializerOptions JsonSerializerOptions,
    bool LogOutOnDispose,
    bool ThrowOnQueryWarning);