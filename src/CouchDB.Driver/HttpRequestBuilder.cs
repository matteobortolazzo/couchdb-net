using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Shared;

namespace CouchDB.Driver;

public class HttpRequestBuilder(HttpClient httpClient, Uri baseUri)
{
    private TimeSpan? _timeout;
    private readonly UriBuilder _uriBuilder = new(baseUri);
    private readonly List<HttpStatusCode> _allowedStatusCodes = [];
    private readonly Dictionary<string, string> _headers = new();

    public HttpRequestBuilder AppendPathSegment(string segment)
    {
        _uriBuilder.Path = _uriBuilder.Path.TrimEnd('/') + "/" + segment.TrimStart('/');
        return this;
    }

    public HttpRequestBuilder AppendPathSegments(params string[] segments)
    {
        foreach (var segment in segments)
        {
            AppendPathSegment(segment);
        }

        return this;
    }

    public HttpRequestBuilder SetQueryParam(string key, object? value)
    {
        NameValueCollection query = System.Web.HttpUtility.ParseQueryString(_uriBuilder.Query);
        query[key] = value?.ToString();
        _uriBuilder.Query = query.ToString();
        return this;
    }

    public HttpRequestBuilder ApplyQueryParametersOptions(object options)
    {
        IEnumerable<(string Name, object? Value)> queryParameters = OptionsHelper.ToQueryParameters(options);
        foreach (var (name, value) in queryParameters)
        {
            SetQueryParam(name, value);
        }

        return this;
    }

    public HttpRequestBuilder AllowHttpStatus(HttpStatusCode statusCode)
    {
        _allowedStatusCodes.Add(statusCode);
        return this;
    }

    public HttpRequestBuilder AllowAnyHttpStatus()
    {
        _allowedStatusCodes.Clear();
        return this;
    }

    public HttpRequestBuilder WithHeader(string name, string value)
    {
        _headers[name] = value;
        return this;
    }

    public HttpRequestBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    public async Task<HttpResponseMessage> HeadAsync(
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Head, _uriBuilder.Uri);
        return await SendAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _uriBuilder.Uri);
        return await SendAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetStringAsync(
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _uriBuilder.Uri);
        HttpResponseMessage response =
            await SendAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await response.GetStringAsync().ConfigureAwait(false);
    }

    public async Task<TResponse> GetJsonAsync<TResponse>(
        JsonSerializerOptions? jsonSerializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _uriBuilder.Uri);
        HttpResponseMessage response =
            await SendAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await response.GetJsonAsync<TResponse>(jsonSerializerOptions).ConfigureAwait(false);
    }

    public async Task<Stream> GetStreamAsync(
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _uriBuilder.Uri);
        HttpResponseMessage response =
            await SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
        return await response.GetStream().ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> PostAsync(
        HttpContent? content = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _uriBuilder.Uri) { Content = content };
        return await SendAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> PostJsonAsync<TBody>(
        TBody body,
        JsonSerializerOptions? jsonSerializerOptions = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default)
    {
        var content = JsonContent.Create(body, options: jsonSerializerOptions);
        var request = new HttpRequestMessage(HttpMethod.Post, _uriBuilder.Uri) { Content = content };
        return await SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> PostStringAsync(
        string body,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default)
    {
        var content = new StringContent(body);
        var request = new HttpRequestMessage(HttpMethod.Post, _uriBuilder.Uri) { Content = content };
        return await SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> PutAsync(
        HttpContent? content = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, _uriBuilder.Uri) { Content = content };
        return await SendAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> PutJsonAsync<TBody>(
        TBody body,
        JsonSerializerOptions? jsonSerializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        var content = JsonContent.Create(body, options: jsonSerializerOptions);
        var request = new HttpRequestMessage(HttpMethod.Put, _uriBuilder.Uri) { Content = content };
        return await SendAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> DeleteAsync(CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, _uriBuilder.Uri);
        return await SendAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default)
    {
        ApplyHeaders(request);

        if (_timeout.HasValue)
        {
            using var timeoutCts = new CancellationTokenSource(_timeout.Value);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            cancellationToken = linkedCts.Token;
        }

        HttpResponseMessage response =
            await httpClient.SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
        await ValidateResponseAsync(response, cancellationToken).ConfigureAwait(false);
        return response;
    }

    private void ApplyHeaders(HttpRequestMessage request)
    {
        foreach (KeyValuePair<string, string> header in _headers)
        {
            if (request.Content != null && IsContentHeader(header.Key))
            {
                request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            else
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }

    internal static bool IsContentHeader(string headerName)
    {
        return headerName.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-Language", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-Location", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-MD5", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-Range", StringComparison.OrdinalIgnoreCase);
    }

    private async Task ValidateResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        if (_allowedStatusCodes.Count > 0 &&
            _allowedStatusCodes.Contains(response.StatusCode))
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new CouchHttpResponseException(
            response.StatusCode,
            content,
            $"Request failed with status code {(int)response.StatusCode} ({response.StatusCode})");
    }
}