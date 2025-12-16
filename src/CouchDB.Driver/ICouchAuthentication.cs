
using System.Threading.Tasks;

namespace CouchDB.Driver;

public interface ICouchAuthentication
{
}

public record BasicCouchAuthentication(string Username, string Password) : ICouchAuthentication;

public record CookieCouchAuthentication(string Username, string Password, int CookiesDuration) : ICouchAuthentication;

public record ProxyCouchAuthentication(string Username, IReadOnlyCollection<string>? Roles = null, string? Token = null)
    : ICouchAuthentication;

public record JwtCouchAuthentication(Func<Task<string>> JwtTokenGenerator) : ICouchAuthentication;