using System.Threading.Tasks;

namespace CouchDB.Driver;

public abstract record CouchCredentials
{
    internal CouchCredentials()
    {
    }
}

public record BasicCredentials(string Username, string Password) : CouchCredentials;

public record CookieCredentials(string Username, string Password, int CookiesDuration) : CouchCredentials;

public record ProxyCredentials(string Username, IReadOnlyCollection<string>? Roles = null, string? Token = null)
    : CouchCredentials;

public record JwtCredentials(Func<Task<string>> JwtTokenGenerator) : CouchCredentials;