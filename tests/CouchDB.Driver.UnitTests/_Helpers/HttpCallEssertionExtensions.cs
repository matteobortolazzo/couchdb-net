using Flurl.Http.Testing;
using System.Text.Json.Serialization;
using System;

namespace CouchDB.Driver.UnitTests._Helpers
{
    public static class HttpCallEssertionExtensions
    {
        public static HttpCallAssertion WithJsonBody<TBody>(this HttpCallAssertion assertion, Func<TBody, bool> assert)
        {
            return assertion
                .WithContentType("application/json")
                .With(call =>
                {
                    var body = JsonConvert.DeserializeObject<TBody>(call.RequestBody);
                    return assert(body);
                });
        }    
    }
}
