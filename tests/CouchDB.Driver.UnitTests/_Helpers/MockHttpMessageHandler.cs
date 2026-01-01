using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace CouchDB.Driver.UnitTests._Helpers;

public class MockHttpMessageHandler(List<LoggedCall> callLog) : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responsesQueue = new();
    private bool _shouldTimeout;

    public void SetResponse(HttpResponseMessage response)
    {
        _responsesQueue.Enqueue(response);
    }

    public void SetResponse(string? content, int statusCode, Dictionary<string, string>? headers = null,
        Dictionary<string, string>? cookies = null)
    {
        _responsesQueue.Enqueue(CreateResponse(content, statusCode, headers, cookies));
    }

    private static HttpResponseMessage CreateResponse(string? content, int statusCode,
        Dictionary<string, string>? headers, Dictionary<string, string>? cookies)
    {
        var response = new HttpResponseMessage((HttpStatusCode)statusCode);

        if (content != null)
        {
            response.Content = new StringContent(content);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        if (headers != null)
        {
            foreach (var header in headers)
            {
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (cookies != null)
        {
            foreach (var cookie in cookies)
            {
                response.Headers.TryAddWithoutValidation("Set-Cookie", $"{cookie.Key}={cookie.Value}");
            }
        }

        return response;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_shouldTimeout)
        {
            _shouldTimeout = false;
            throw new TaskCanceledException("The request was canceled due to the configured timeout.");
        }

        var requestBody = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken) : null;
        callLog.Add(new LoggedCall(request, requestBody));

        if (_responsesQueue.Count > 0)
        {
            return _responsesQueue.Dequeue();
        }

        throw new InvalidOperationException("No response configured for the request.");
    }

    protected override void Dispose(bool disposing)
    {
        _responsesQueue.Clear();
        base.Dispose(disposing);
    }
}