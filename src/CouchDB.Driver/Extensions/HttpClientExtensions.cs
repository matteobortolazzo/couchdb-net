using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CouchDB.Driver.Extensions;

public static class HttpClientExtensions
{
    extension(Task<HttpResponseMessage> responseTask)
    {
        public async Task<T> ReceiveJson<T>(JsonSerializerOptions? options = null)
        {
            using HttpResponseMessage response = await responseTask.ConfigureAwait(false);
            return await response.GetJsonAsync<T>(options).ConfigureAwait(false);
        }

        public async Task<Stream> ReceiveStream()
        {
            using HttpResponseMessage response = await responseTask.ConfigureAwait(false);
            Stream originalStream = await response.GetStream().ConfigureAwait(false);
#if DEBUG
            // Used for unit testing
            var memoryStream = new MemoryStream();
            await originalStream.CopyToAsync(memoryStream).ConfigureAwait(false);
            memoryStream.Position = 0;
            return memoryStream;
#else
    return originalStream;
#endif
        }
    }

    extension(HttpResponseMessage response)
    {
        public async Task<T> GetJsonAsync<T>(JsonSerializerOptions? options = null)
        {
            T? result = await response.Content.ReadFromJsonAsync<T>(options).ConfigureAwait(false);
            return result ?? throw new InvalidOperationException(
                "Failed to deserialize response content to the specified type.");
        }

        public async Task<string> GetStringAsync()
        {
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<Stream> GetStream()
        {
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }
    }

    public static HttpRequestBuilder Request(this HttpClient client, Uri baseUri)
    {
        return new HttpRequestBuilder(client, baseUri);
    }
}