using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace CouchDB.Driver.UnitTests._Helpers;

public class HttpCallAssertion(List<LoggedCall> callLog, string expectedUrl)
{
    private readonly List<Func<LoggedCall, (bool Success, string ErrorMessage)>> _checks = [];

    public HttpCallAssertion WithVerb(HttpMethod method)
    {
        _checks.Add(call =>
        {
            var success = call.Request.Method == method;
            return (success, success ? string.Empty : $"Expected method {method}, but was {call.Request.Method}");
        });
        Verify();
        return this;
    }

    public HttpCallAssertion WithHeader(string name, string value)
    {
        _checks.Add(call =>
        {
            // Check content headers
            if (HttpRequestBuilder.IsContentHeader(name))
            {
                if (call.Request.Content?.Headers.TryGetValues(name, out var contentValues) == true &&
                    contentValues.Contains(value))
                {
                    return (true, string.Empty);
                }
            }
            // Check request headers
            else if (call.Request.Headers.TryGetValues(name, out var values) && values.Contains(value))
            {
                return (true, string.Empty);
            }

            return (false, $"Expected header '{name}' with value '{value}', but it was not found");
        });
        Verify();
        return this;
    }

    public HttpCallAssertion WithQueryParam(string name, object value)
    {
        _checks.Add(call =>
        {
            var query = call.Request.RequestUri?.Query;
            var expected = $"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value.ToString() ?? string.Empty)}";

            if (string.IsNullOrEmpty(query) || !query.Contains(expected, StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"Expected query parameter '{name}={value}', but it was not found");
            }

            return (true, string.Empty);
        });
        Verify();
        return this;
    }

    public HttpCallAssertion WithContentType(string contentType)
    {
        return WithHeader("Content-Type", contentType);
    }

    public HttpCallAssertion WithoutQueryParam(string name)
    {
        _checks.Add(call =>
        {
            var query = call.Request.RequestUri?.Query;
            if (!string.IsNullOrEmpty(query) &&
                query.Contains($"{Uri.EscapeDataString(name)}=", StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"Expected query parameter '{name}' to NOT exist, but it was found");
            }

            return (true, string.Empty);
        });
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
        _checks.Add(call =>
        {
            var success = call.RequestBody == body;
            return (success, success ? string.Empty : $"Expected body '{body}', but was '{call.RequestBody}'");
        });
        Verify();
        return this;
    }

    public HttpCallAssertion WithRequestJson<T>(T body)
    {
        var json = JsonSerializer.Serialize(body);
        _checks.Add(call =>
        {
            var success = call.RequestBody == json;
            return (success, success ? string.Empty : $"Expected JSON body '{json}', but was '{call.RequestBody}'");
        });
        Verify();
        return this;
    }

    public HttpCallAssertion WithJsonBody<TBody>(Func<TBody, bool> assert)
    {
        return With(call =>
        {
            if (call.RequestBody == null)
            {
                return false;
            }

            var body = JsonSerializer.Deserialize<TBody>(call.RequestBody, JsonSerializerOptions.Web)!;
            return assert(body);
        });
    }

    public HttpCallAssertion With(Func<LoggedCall, bool> predicate)
    {
        _checks.Add(call =>
        {
            var success = predicate(call);
            return (success, success ? string.Empty : "Custom predicate check failed");
        });
        Verify();
        return this;
    }

    private void Verify()
    {
        var firstFailure = GetFirstFailure();

        if (firstFailure == null)
        {
            return;
        }

        var errorMsg = _checks.Count > 0
            ? $"Expected call to {expectedUrl} not found with the specified criteria. First failure: {firstFailure}"
            : $"Expected call to {expectedUrl} not found.";
        Assert.False(_checks.Count > 0, errorMsg);
    }

    private string? GetFirstFailure()
    {
        var matchingCalls = new List<(LoggedCall Call, string ErrorMessage)>();

        foreach (var call in callLog)
        {
            if (!UrlMatches(call.Request.RequestUri?.ToString(), expectedUrl))
                continue;

            var failedCheck = false;
            foreach (var check in _checks)
            {
                var (success, errorMessage) = check(call);
                if (!success)
                {
                    matchingCalls.Add((call, errorMessage));
                    failedCheck = true;
                    break;
                }
            }

            if (!failedCheck)
            {
                return null; // Found a fully matching call
            }
        }

        if (matchingCalls.Count > 0)
        {
            return matchingCalls[0].ErrorMessage;
        }

        return _checks.Count > 0 ? "No matching URL found" : null;
    }

    private static bool UrlMatches(string? actualUrl, string expectedUrl)
    {
        if (actualUrl == null)
        {
            return false;
        }

        var actualBaseUrl = actualUrl.Split('?')[0];
        var expectedBaseUrl = expectedUrl.Split('?')[0];
        return expectedBaseUrl == actualBaseUrl;
    }
}