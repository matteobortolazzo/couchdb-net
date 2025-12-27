using System.Threading.Tasks;

namespace CouchDB.Driver;

public abstract record CouchCredentials
{
    internal CouchCredentials()
    {
    }
}

public record BasicCredentials(string Username, string Password) : CouchCredentials;

public record CookieCredentials(string Username, string Password, int CookiesDuration = 10) : CouchCredentials;

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

    public string? JwtToken { get; init; }
    public Func<Task<string>>? JwtTokenGenerator { get; init; }

    public void Deconstruct(out string? jwtToken, out Func<Task<string>>? jwtTokenGenerator)
    {
        jwtToken = JwtToken;
        jwtTokenGenerator = JwtTokenGenerator;
    }
}