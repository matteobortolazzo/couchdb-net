using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

namespace CouchDB.Driver.UnitTests._Helpers;

public sealed class HttpTestHelper : IDisposable
{
    private readonly MockHttpMessageHandler _handler;
    public HttpClient HttpClient { get; }
    public List<LoggedCall> CallLog { get; } = [];

    public HttpTestHelper()
    {
        _handler = new MockHttpMessageHandler(CallLog);
        HttpClient = new HttpClient(_handler);
    }

    public void SimulateTimeout()
    {
        _handler.SimulateTimeout();
    }

    public void RespondWith(string content, int statusCode = 200, Dictionary<string, string>? headers = null,
        Dictionary<string, string>? cookies = null)
    {
        _handler.SetResponse(content, statusCode, headers, cookies);
    }

    public void RespondWithJson<T>(T content, int statusCode = 200, Dictionary<string, string>? headers = null,
        Dictionary<string, string>? cookies = null)
    {
        var stringContent = JsonSerializer.Serialize(content);
        headers ??= new Dictionary<string, string>();
        headers.TryAdd("Content-Type", "application/json");
        _handler.SetResponse(stringContent, statusCode, headers, cookies);
    }

    public HttpCallAssertion ShouldHaveCalled(string url)
    {
        return new HttpCallAssertion(CallLog, url);
    }

    public void Dispose()
    {
        HttpClient.Dispose();
    }
}

public record LoggedCall(HttpRequestMessage Request, string? RequestBody);