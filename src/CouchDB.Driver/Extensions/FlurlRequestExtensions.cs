using System.Net;
using System.Net.Http;
using CouchDB.Driver.Shared;

namespace CouchDB.Driver.Extensions;

internal static class FlurlRequestExtensions
{
    /// <param name="request">The HttpRequestBuilder instance.</param>
    extension(HttpRequestBuilder request)
    {
        public HttpRequestBuilder ApplyQueryParametersOptions(object options)
        {
            IEnumerable<(string Name, object? Value)> queryParameters = OptionsHelper.ToQueryParameters(options);
            foreach (var (name, value) in queryParameters)
            {
                request = request.SetQueryParam(name, value);
            }

            return request;
        }
    }

    public static bool IsSuccessful(this HttpResponseMessage response)
    {
        return
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created ||
            response.StatusCode == HttpStatusCode.Accepted ||
            response.StatusCode == HttpStatusCode.NoContent;
    }
}