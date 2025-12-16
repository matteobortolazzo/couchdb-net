
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CouchDB.Driver.Options;

public class CouchOptionsBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CouchOptionsBuilder"/> class with no options set.
    /// </summary>
    public CouchOptionsBuilder() : this(new CouchOptions<CouchContext>()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CouchOptionsBuilder"/> class to further configure a given <see cref="CouchOptions"/>.
    /// </summary>
    /// <param name="options">The options to be configured.</param>
    public CouchOptionsBuilder(CouchOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        Options = options;
    }

    /// <summary>
    /// Instance of the options built.
    /// </summary>
    public virtual CouchOptions Options { get; }

    /// <summary>
    /// Set the database endpoint.
    /// </summary>
    /// <param name="endpoint">The database endpoint.</param>
    /// <returns>Return the current instance to chain calls.</returns>
    public virtual CouchOptionsBuilder UseEndpoint(string endpoint)
    {
        Options.Endpoint = new Uri(endpoint);
        return this;
    }

    /// <summary>
    /// Set the database endpoint.
    /// </summary>
    /// <param name="endpointUri">The database endpoint.</param>
    /// <returns>Return the current instance to chain calls.</returns>
    public virtual CouchOptionsBuilder UseEndpoint(Uri endpointUri)
    {
        Options.Endpoint = endpointUri;
        return this;
    }

    /// <summary>
    /// If in a <see cref="CouchContext"/>, ensure that all databases exist.
    /// Ignore in the <see cref="CouchClient"/>.
    /// </summary>
    /// <returns>Return the current instance to chain calls.</returns>
    public virtual CouchOptionsBuilder EnsureDatabaseExists()
    {
        Options.CheckDatabaseExists = true;
        return this;
    }

    /// <summary>
    /// If in a <see cref="CouchContext"/>, overrides indexes with same design document and name.
    /// Ignore in the <see cref="CouchClient"/>.
    /// </summary>
    /// <returns>Return the current instance to chain calls.</returns>
    public virtual CouchOptionsBuilder OverrideExistingIndexes()
    {
        Options.OverrideExistingIndexes = true;
        return this;
    }

    /// <summary>
    /// Enables basic authentication. 
    /// Basic authentication (RFC 2617) is a quick and simple way to authenticate with CouchDB. The main drawback is the need to send user credentials with each request which may be insecure and could hurt operation performance (since CouchDB must compute the password hash with every request).
    /// </summary>
    /// <param name="username">Server username.</param>
    /// <param name="password">Server password.</param>
    /// <returns>Return the current instance to chain calls.</returns>
    public virtual CouchOptionsBuilder UseBasicAuthentication(string username, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        Options.Authentication = new BasicCouchAuthentication(username, password);
        return this;
    }


    /// <summary>
    /// Enables cookie authentication. 
    /// For cookie authentication (RFC 2109) CouchDB generates a token that the client can use for the next few requests to CouchDB. Tokens are valid until a timeout.
    /// </summary>
    /// <param name="username">Server username.</param>
    /// <param name="password">Server password.</param>
    /// <param name="cookieDuration">Cookie duration in minutes.</param>
    /// <returns>Return the current instance to chain calls.</returns>
    public virtual CouchOptionsBuilder UseCookieAuthentication(string username, string password, int cookieDuration = 10)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        if (cookieDuration < 1)
        {
            throw new ArgumentException("Cookie duration must be greater than zero.", nameof(cookieDuration));
        }

        Options.Authentication = new CookieCouchAuthentication(username, password, cookieDuration);
        return this;
    }

    /// <summary>
    /// Enables proxy authentication. 
    /// </summary>
    /// <param name="username">Server username.</param>
    /// <param name="roles">Server roles.</param>
    /// <param name="token">Computed authentication token.</param>
    /// <returns>Return the current instance to chain calls.</returns>
    public virtual CouchOptionsBuilder UseProxyAuthentication(string username, IReadOnlyCollection<string> roles, string? token = null)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(roles);

        Options.Authentication = new ProxyCouchAuthentication(username,  roles, token);
        return this;
    }

    /// <summary>
    /// Enables JWT authentication. 
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>Return the current instance to chain calls.</returns>
    public virtual CouchOptionsBuilder UseJwtAuthentication(string token)
    {
        return UseJwtAuthentication(() => Task.FromResult(token));
    }

    /// <summary>
    /// Enables JWT authentication. The function is called before each call.
    /// </summary>
    /// <param name="tokenGenerator">Function that returns a JWT token asynchronous.</param>
    /// <returns>Return the current instance to chain calls.</returns>
    public virtual CouchOptionsBuilder UseJwtAuthentication(Func<Task<string>> tokenGenerator)
    {
        Options.Authentication = new JwtCouchAuthentication(tokenGenerator);
        return this;
    }

    /// <summary>
    /// Set the field to use to identify document types. Default: <c>split_discriminator</c>.
    /// </summary>
    /// <param name="databaseSplitDiscriminator">The document field to use as discriminator.</param>
    /// <returns>Return the current instance to chain calls.</returns>
    public virtual CouchOptionsBuilder WithDatabaseSplitDiscriminator(string databaseSplitDiscriminator)
    {
        Options.DatabaseSplitDiscriminator = databaseSplitDiscriminator;
        return this;
    }

    /// <summary>
    /// Disables log out on client dispose. 
    /// </summary>
    /// <returns>Return the current instance to chain calls.</returns>
    public virtual CouchOptionsBuilder DisableLogOutOnDispose()
    {
        Options.LogOutOnDispose = false;
        return this;
    }
    
    /// <summary>
    /// Throw an exception if a query warning is returned from the server.
    /// </summary>
    /// <returns></returns>
    public virtual CouchOptionsBuilder ThrowOnQueryWarning()
    {
        Options.ThrowOnQueryWarning = true;
        return this;
    }
}