using Flurl.Http.Testing;
using System;
using System.Text.Json;

namespace CouchDB.Driver.UnitTests._Helpers;

public static class HttpCallAssertionExtensions
{
    public static HttpCallAssertion WithJsonBody<TBody>(this HttpCallAssertion assertion, Func<TBody, bool> assert)
    {
        return assertion
            .WithContentType("application/json")
            .With(call =>
            {
                var body = JsonSerializer.Deserialize<TBody>(call.RequestBody);
                return assert(body);
            });
    }
}