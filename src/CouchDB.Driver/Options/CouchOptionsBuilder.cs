using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CouchDB.Driver.Helpers;
using Flurl.Http.Configuration;
using Newtonsoft.Json;

namespace CouchDB.Driver.Options
{
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
            Check.NotNull(options, nameof(options));

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
            Check.NotNull(username, nameof(username));
            Check.NotNull(password, nameof(password));

            Options.AuthenticationType = AuthenticationType.Basic;
            Options.Username = username;
            Options.Password = password;
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
            Check.NotNull(username, nameof(username));
            Check.NotNull(password, nameof(password));

            if (cookieDuration < 1)
            {
                throw new ArgumentException("Cookie duration must be greater than zero.", nameof(cookieDuration));
            }

            Options.AuthenticationType = AuthenticationType.Cookie;
            Options.Username = username;
            Options.Password = password;
            Options.CookiesDuration = cookieDuration;
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
            Check.NotNull(username, nameof(username));
            Check.NotNull(roles, nameof(roles));

            Options.AuthenticationType = AuthenticationType.Proxy;
            Options.Username = username;
            Options.Roles = roles;
            Options.Password = token;
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
            Options.AuthenticationType = AuthenticationType.Jwt;
            Options.JwtTokenGenerator = tokenGenerator;
            return this;
        }

        /// <summary>
        /// Disables documents pluralization in requests.
        /// </summary>
        /// <returns>Return the current instance to chain calls.</returns>
        public virtual CouchOptionsBuilder DisableDocumentPluralization()
        {
            Options.PluralizeEntities = false;
            return this;
        }

        /// <summary>
        /// Sets the format case for documents. Default: underscore_case.
        /// </summary>
        /// <param name="type">The type of case format.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public virtual CouchOptionsBuilder SetDocumentCase(DocumentCaseType type)
        {
            Options.DocumentsCaseType = type;
            return this;
        }

        /// <summary>
        /// Sets the format case for properties. Default: camelCase.
        /// </summary>
        /// <param name="type">The type of case format.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public virtual CouchOptionsBuilder SetPropertyCase(PropertyCaseType type)
        {
            Options.PropertiesCase = type;
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
        /// Sets how to handle null values during serialization.
        /// </summary>
        /// <param name="nullValueHandling">The type of null value handling.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public virtual CouchOptionsBuilder SetJsonNullValueHandling(NullValueHandling nullValueHandling)
        {
            Options.NullValueHandling = nullValueHandling;
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
        /// Removes any SSL certificate validation.
        /// </summary>
        /// <returns>Return the current instance to chain calls.</returns>
        public virtual CouchOptionsBuilder IgnoreCertificateValidation()
        {
            Options.ServerCertificateCustomValidationCallback = (m, x, c, s) => true;
            return this;
        }

        /// <summary>
        /// Sets a custom SSL validation rule.
        /// </summary>
        /// <param name="serverCertificateCustomValidationCallback">SSL validation function</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public virtual CouchOptionsBuilder ConfigureCertificateValidation(Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>
            serverCertificateCustomValidationCallback)
        {
            Check.NotNull(serverCertificateCustomValidationCallback, nameof(serverCertificateCustomValidationCallback));
            Options.ServerCertificateCustomValidationCallback = serverCertificateCustomValidationCallback;
            return this;
        }

        /// <summary>
        /// Configure the Flurl client.
        /// </summary>
        /// <param name="flurlSettingsAction">An action representing to configure Flurl.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public virtual CouchOptionsBuilder ConfigureFlurlClient(Action<ClientFlurlHttpSettings> flurlSettingsAction)
        {
            Options.ClientFlurlHttpSettingsAction = flurlSettingsAction;
            return this;
        }
    }
}