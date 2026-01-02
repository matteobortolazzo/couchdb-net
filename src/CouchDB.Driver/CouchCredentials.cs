using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CouchDB.Driver;

public abstract record CouchCredentials
{
    internal CouchCredentials()
    {
    }
}

public record BasicCredentials : CouchCredentials
{
    public BasicCredentials(string Username, string Password)
    {
        this.Username = Username;
        this.Password = Password;
        Token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"));
    }

    public string Username { get; }
    public string Password { get; }
    internal string Token { get; }
}

public record CookieCredentials(string Username, string Password, int CookiesDuration = 10) : CouchCredentials
{
    private DateTimeOffset? _cookieCreationDate;
    private string? _cookieToken;
    private readonly SemaphoreSlim _loginLock = new(1, 1);

    public async Task<string> GetTokenAsync(HttpClient httpClient, Uri baseUri, CancellationToken cancellationToken)
    {
        if (!IsTokenExpired())
        {
            return _cookieToken!;
        }

        await _loginLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!IsTokenExpired())
            {
                return _cookieToken!;
            }

            _cookieToken = await LoginAsync(httpClient, baseUri, cancellationToken).ConfigureAwait(false);
            return _cookieToken;
        }
        finally
        {
            _loginLock.Release();
        }
    }

    private bool IsTokenExpired()
    {
        return
            _cookieToken == null ||
            !_cookieCreationDate.HasValue ||
            _cookieCreationDate.Value.AddMinutes(CookiesDuration) < DateTimeOffset.UtcNow;
    }

    private async Task<string> LoginAsync(HttpClient httpClient, Uri baseUri, CancellationToken cancellationToken)
    {
        var loginRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUri, "_session"))
        {
            Content = JsonContent.Create(new { name = Username, password = Password })
        };

        HttpResponseMessage response =
            await httpClient.SendAsync(loginRequest, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        _cookieCreationDate = DateTimeOffset.UtcNow;

        if (!response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookies))
        {
            throw new InvalidOperationException("Error while trying to log-in: No cookies received.");
        }

        var authSessionCookie = cookies
            .Select(c => c.Split(';')[0])
            .FirstOrDefault(c => c.StartsWith("AuthSession="));

        if (authSessionCookie != null)
        {
            return authSessionCookie.Split('=')[1];
        }

        throw new InvalidOperationException("Error while trying to log-in: AuthSession cookie not found.");
    }
}

public record ProxyCredentials(string Username, IReadOnlyCollection<string>? Roles = null, string? Token = null)
    : CouchCredentials;

public record JwtCredentials : CouchCredentials
{
    public JwtCredentials(string jwtToken)
    {
        JwtToken = jwtToken;
        JwtTokenGenerator = null;
    }

    public JwtCredentials(Func<Task<string>> jwtTokenGenerator)
    {
        JwtToken = null;
        JwtTokenGenerator = jwtTokenGenerator;
    }

    public string? JwtToken { get; }
    public Func<Task<string>>? JwtTokenGenerator { get; }
}