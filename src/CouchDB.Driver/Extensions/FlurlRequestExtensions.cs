using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Shared;
using Flurl.Http;
using Flurl.Http.Content;

namespace CouchDB.Driver.Extensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "<Pending>")]
    internal static class FlurlRequestExtensions
    {
        /// <summary>Sends an asynchronous POST request.</summary>
        /// <param name="request">The IFlurlRequest instance.</param>
        /// <param name="data">Data to parse.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="completionOption">The HttpCompletionOption used in the request. Optional.</param>
        /// <returns>A Task whose result is the response body as a Stream.</returns>
        public static Task<Stream> PostJsonStreamAsync(
            this IFlurlRequest request,
            object data,
            CancellationToken cancellationToken = default,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            using var capturedJsonContent = new CapturedJsonContent(request.Settings.JsonSerializer.Serialize(data));
            return request.SendAsync(HttpMethod.Post, (HttpContent)capturedJsonContent, cancellationToken, completionOption).ReceiveStream();
        }


        /// <summary>Sends an asynchronous POST request.</summary>
        /// <param name="request">The IFlurlRequest instance.</param>
        /// <param name="data">Data to parse.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="completionOption">The HttpCompletionOption used in the request. Optional.</param>
        /// <returns>A Task whose result is the response body as a Stream.</returns>
        public static Task<Stream> PostStringStreamAsync(
            this IFlurlRequest request,
            string data,
            CancellationToken cancellationToken = default,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            using var capturedStringContent = new CapturedStringContent(data);
            return request.SendAsync(HttpMethod.Post, capturedStringContent, cancellationToken, completionOption).ReceiveStream();
        }

        public static IFlurlRequest ApplyQueryParametersOptions(this IFlurlRequest request, object options)
        {
            var queryParameters = OptionsHelper.ToQueryParameters(options);
            foreach (var (name, value) in queryParameters)
            {
                request = request.SetQueryParam(name, value);
            }

            return request;
        }
    }
}
