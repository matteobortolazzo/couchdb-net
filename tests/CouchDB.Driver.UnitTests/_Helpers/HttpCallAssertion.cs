using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace CouchDB.Driver.UnitTests._Helpers;

public class HttpCallAssertion(List<LoggedCall> callLog, string expectedUrl)
{
    private class QueryParamShouldNotExist
    {
    }

    private HttpMethod? _expectedMethod;
    private readonly Dictionary<string, string> _expectedHeaders = new();
    private readonly Dictionary<string, object> _expectedQueryParams = new();
    private string? _expectedBody;
    private Func<LoggedCall, bool>? _predicate;

    public HttpCallAssertion WithVerb(HttpMethod method)
    {
        _expectedMethod = method;
        return this;
    }

    public HttpCallAssertion WithHeader(string name, string value)
    {
        _expectedHeaders[name] = value;
        Verify();
        return this;
    }

    public HttpCallAssertion WithQueryParam(string name, object value)
    {
        _expectedQueryParams[name] = value;
        Verify();
        return this;
    }

    public HttpCallAssertion WithContentType(string contentType)
    {
        return WithHeader("Content-Type", contentType);
    }

    public HttpCallAssertion WithoutQueryParam(string name)
    {
        // Use a special marker to indicate this param should NOT exist
        _expectedQueryParams[name] = new QueryParamShouldNotExist();
        Verify();
        return this;
    }

    public HttpCallAssertion WithBasicAuth(string username, string password)
    {
        var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
        return WithHeader("Authorization", $"Basic {credentials}");
    }

    public HttpCallAssertion WithRequestBody(string body)
    {
        _expectedBody = body;
        Verify();
        return this;
    }

    public HttpCallAssertion WithRequestJson<T>(T body)
    {
        var json = JsonSerializer.Serialize(body);
        _expectedBody = json;
        Verify();
        return this;
    }

    public HttpCallAssertion With(Func<LoggedCall, bool> predicate)
    {
        _predicate = predicate;
        Verify();
        return this;
    }

    private void Verify()
    {
        var matchingCall = callLog.Find(call =>
        {
            if (!UrlMatches(call.Request.RequestUri?.ToString(), expectedUrl))
                return false;

            if (_expectedMethod != null && call.Request.Method != _expectedMethod)
                return false;

            if (_expectedBody != null && call.RequestBody != _expectedBody)
            {
                return false;
            }

            foreach (var header in _expectedHeaders)
            {
                if (!call.Request.Headers.TryGetValues(header.Key, out var values) ||
                    !values.Contains(header.Value))
                    return false;
            }

            foreach (var queryParam in _expectedQueryParams)
            {
                var query = call.Request.RequestUri?.Query;

                if (queryParam.Value is QueryParamShouldNotExist)
                {
                    if (!string.IsNullOrEmpty(query) && query.Contains($"{queryParam.Key}="))
                    {
                        return false;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(query) || !query.Contains($"{queryParam.Key}={queryParam.Value}"))
                    {
                        return false;
                    }
                }
            }

            if (_predicate != null && !_predicate(call))
            {
                return false;
            }

            return true;
        });

        if (matchingCall == null)
        {
            throw new InvalidOperationException(
                $"Expected call to {expectedUrl} not found with the specified criteria.");
        }
    }

    private static bool UrlMatches(string? actualUrl, string expectedUrl)
    {
        if (actualUrl == null)
        {
            return false;
        }

        if (!expectedUrl.Contains('*'))
        {
            return actualUrl == expectedUrl;
        }

        var pattern = expectedUrl.TrimEnd('*');
        return actualUrl.StartsWith(pattern, StringComparison.Ordinal);
    }
}