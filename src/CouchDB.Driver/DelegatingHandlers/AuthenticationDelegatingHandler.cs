using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CouchDB.Driver.DelegatingHandlers;

public class AuthenticationDelegatingHandler(ICouchAuthentication? authentication) : DelegatingHandler
{
    private DateTimeOffset? _cookieCreationDate;
    private string? _cookieToken;
    private readonly SemaphoreSlim _loginLock = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (authentication == null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        // If session requests no authorization needed
        if (request.RequestUri?.ToString()
                .Contains("_session", StringComparison.InvariantCultureIgnoreCase) ==
            true)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        Type authType = authentication.GetType();
        if (authType == typeof(BasicCouchAuthentication))
        {
            var auth = (BasicCouchAuthentication)authentication;
            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{auth.Username}:{auth.Password}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            return await base.SendAsync(request, cancellationToken);
        }

        if (authType == typeof(CookieCouchAuthentication))
        {
            var auth = (CookieCouchAuthentication)authentication;
            var isTokenExpired =
                !_cookieCreationDate.HasValue ||
                _cookieCreationDate.Value.AddMinutes(auth.CookiesDuration) < DateTimeOffset.UtcNow;
            if (isTokenExpired)
            {
                await _loginLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    // Double-check after acquiring lock
                    if (!_cookieCreationDate.HasValue ||
                        _cookieCreationDate.Value.AddMinutes(auth.CookiesDuration) < DateTimeOffset.UtcNow)
                    {
                        await LoginAsync(request, auth, cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    _loginLock.Release();
                }
            }

            request.Headers.Add("Cookie", $"AuthSession={_cookieToken}");
            return await base.SendAsync(request, cancellationToken);
        }

        if (authType == typeof(ProxyCouchAuthentication))
        {
            var auth = (ProxyCouchAuthentication)authentication;
            request.Headers.Add("X-Auth-CouchDB-UserName", auth.Username);
            request.Headers.Add("X-Auth-CouchDB-Roles", string.Join(",", auth.Roles ?? Array.Empty<string>()));
            if (auth.Token != null)
            {
                request.Headers.Add("X-Auth-CouchDB-Token", auth.Token);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        if (authType == typeof(JwtCouchAuthentication))
        {
            var auth = (JwtCouchAuthentication)authentication;
            var jwt = await auth.JwtTokenGenerator().ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            return await base.SendAsync(request, cancellationToken);
        }

        throw new NotSupportedException($"Authentication of type {authType.Name} is not supported.");
    }

    public void ClearCookie()
    {
        _cookieCreationDate = null;
        _cookieToken = null;
    }
    
    private async Task LoginAsync(HttpRequestMessage originalRequest, CookieCouchAuthentication auth,
        CancellationToken cancellationToken)
    {
        var baseUri = new Uri(originalRequest.RequestUri!.GetLeftPart(UriPartial.Authority));
        var loginRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUri, "_session"))
        {
            Content = JsonContent.Create(new { name = auth.Username, password = auth.Password })
        };

        HttpResponseMessage response = await base.SendAsync(loginRequest, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        _cookieCreationDate = DateTimeOffset.UtcNow;

        if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookies))
        {
            var authSessionCookie = cookies
                .Select(c => c.Split(';')[0])
                .FirstOrDefault(c => c.StartsWith("AuthSession="));

            if (authSessionCookie != null)
            {
                _cookieToken = authSessionCookie.Split('=')[1];
            }
            else
            {
                throw new InvalidOperationException("Error while trying to log-in: AuthSession cookie not found.");
            }
        }
        else
        {
            throw new InvalidOperationException("Error while trying to log-in: No cookies received.");
        }
    }
}