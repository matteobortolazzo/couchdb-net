using System.Net;
using System.Net.Http;

namespace CouchDB.Driver.Exceptions;

public class CouchHttpResponseException(HttpStatusCode statusCode, string? responseContent, string message)
    : HttpRequestException(message, null, statusCode)
{
    public string? ResponseContent { get; } = responseContent;
}