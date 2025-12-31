using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CouchDB.Driver.UnitTests._Helpers;

public class MockHttpMessageHandler(List<LoggedCall> callLog) : HttpMessageHandler
{
    private bool _shouldTimeout;
    private string? _responseContent;
    private int _statusCode = 200;
    private Dictionary<string, string>? _responseHeaders;
    private Dictionary<string, string>? _responseCookies;

    public void SetResponse(string content, int statusCode, Dictionary<string, string>? headers = null,
        Dictionary<string, string>? cookies = null)
    {
        _responseContent = content;
        _statusCode = statusCode;
        _responseHeaders = headers;
        _responseCookies = cookies;
    }

    public void SimulateTimeout()
    {
        _shouldTimeout = true;
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

        var response = new HttpResponseMessage((System.Net.HttpStatusCode)_statusCode)
        {
            Content = new StringContent(_responseContent ?? string.Empty)
        };

        if (_responseHeaders != null)
        {
            foreach (var header in _responseHeaders)
            {
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (_responseCookies != null)
        {
            foreach (var cookie in _responseCookies)
            {
                response.Headers.TryAddWithoutValidation("Set-Cookie", $"{cookie.Key}={cookie.Value}");
            }
        }

        return response;
    }
}