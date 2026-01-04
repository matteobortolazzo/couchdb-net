using System.Net.Http;
using System.Text.Json;

namespace CouchDB.Driver.Options;

/// <summary>
/// Options for configuring a <see cref="CouchClient"/>.
/// </summary>
public record CouchClientOptions
{
    /// <summary>
    /// Clear the session by logging out when the client is disposed.
    /// </summary>
    public bool LogOutOnDispose { get; set; } = true;
    
    /// <summary>
    /// Throw an exception when a query returns a warning from the server.
    /// </summary>
    public bool ThrowOnQueryWarning { get; set; } = true;

    /// <summary>
    /// Custom JSON serializer options for serializing and deserializing documents.
    /// </summary>
    [JsonIgnore]
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Custom HttpClient to be used by the CouchClient.
    /// </summary>
    [JsonIgnore]
    public HttpClient? HttpClient { get; set; }
}

internal record InternalCouchClientOptions(
    CouchCredentials Credentials,
    JsonSerializerOptions DocumentJsonSerializerOptions,
    bool LogOutOnDispose,
    bool ThrowOnQueryWarning);