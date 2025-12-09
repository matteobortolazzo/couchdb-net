using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Shared;
using Flurl.Http;
using Flurl.Http.Content;

namespace CouchDB.Driver.Extensions;

internal static class FlurlRequestExtensions
{
    /// <param name="request">The IFlurlRequest instance.</param>
    extension(IFlurlRequest request)
    {
        /// <summary>Sends an asynchronous POST request.</summary>
        /// <param name="data">Data to parse.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="completionOption">The HttpCompletionOption used in the request. Optional.</param>
        /// <returns>A Task whose result is the response body as a Stream.</returns>
        public Task<Stream> PostJsonStreamAsync(object data,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
            CancellationToken cancellationToken = default)
        {
            var capturedJsonContent = new CapturedJsonContent(request.Settings.JsonSerializer.Serialize(data));
            return request.SendAsync(HttpMethod.Post, capturedJsonContent, completionOption, cancellationToken)
                .ReceiveStream();
        }

        /// <summary>Sends an asynchronous POST request.</summary>
        /// <param name="data">Data to parse.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="completionOption">The HttpCompletionOption used in the request. Optional.</param>
        /// <returns>A Task whose result is the response body as a Stream.</returns>
        public Task<Stream> PostStringStreamAsync(string data,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
            CancellationToken cancellationToken = default)
        {
            var capturedStringContent = new CapturedStringContent(data);
            return request.SendAsync(HttpMethod.Post, capturedStringContent, completionOption, cancellationToken)
                .ReceiveStream();
        }

        public IFlurlRequest ApplyQueryParametersOptions(object options)
        {
            IEnumerable<(string Name, object? Value)> queryParameters = OptionsHelper.ToQueryParameters(options);
            foreach (var (name, value) in queryParameters)
            {
                request = request.SetQueryParam(name, value);
            }

            return request;
        }
    }

    public static bool IsSuccessful(this IFlurlResponse response)
    {
        return
            response.StatusCode == (int)HttpStatusCode.OK ||
            response.StatusCode == (int)HttpStatusCode.Created ||
            response.StatusCode == (int)HttpStatusCode.Accepted ||
            response.StatusCode == (int)HttpStatusCode.NoContent;
    }
}