using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using Newtonsoft.Json;

namespace CouchDB.Driver.Options
{
    public class CouchOptionsBuilder<TContext> : CouchOptionsBuilder
        where TContext : CouchContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CouchOptionsBuilder{TContext}"/> class with no options set.
        /// </summary>
        public CouchOptionsBuilder() : this(new CouchOptions<TContext>()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CouchOptionsBuilder{TContext}"/> class to further configure a given <see cref="CouchOptions{TContext}"/>.
        /// </summary>
        /// <param name="options">The options to be configured.</param>
        public CouchOptionsBuilder(CouchOptions<TContext> options) : base(options) { }

        /// <summary>
        /// Instance of the options built.
        /// </summary>
        public new virtual CouchOptions<TContext> Options => (CouchOptions<TContext>)base.Options;

        /// <summary>
        /// Set the database endpoint.
        /// </summary>
        /// <param name="endpoint">The database endpoint.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> UseEndpoint(string endpoint)
            => (CouchOptionsBuilder<TContext>)base.UseEndpoint(endpoint);

        /// <summary>
        /// Set the database endpoint.
        /// </summary>
        /// <param name="endpointUri">The database endpoint.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> UseEndpoint(Uri endpointUri)
            => (CouchOptionsBuilder<TContext>)base.UseEndpoint(endpointUri);

        /// <summary>
        /// If in a <see cref="CouchContext"/>, ensure that all databases exist.
        /// Ignore in the <see cref="CouchClient"/>.
        /// </summary>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> EnsureDatabaseExists()
            => (CouchOptionsBuilder<TContext>)base.EnsureDatabaseExists();

        /// <summary>
        /// If in a <see cref="CouchContext"/>, overrides indexes with same design document and name.
        /// Ignore in the <see cref="CouchClient"/>.
        /// </summary>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> OverrideExistingIndexes()
            => (CouchOptionsBuilder<TContext>)base.OverrideExistingIndexes();

        /// <summary>
        /// Enables basic authentication. 
        /// Basic authentication (RFC 2617) is a quick and simple way to authenticate with CouchDB. The main drawback is the need to send user credentials with each request which may be insecure and could hurt operation performance (since CouchDB must compute the password hash with every request).
        /// </summary>
        /// <param name="username">Server username.</param>
        /// <param name="password">Server password.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> UseBasicAuthentication(string username, string password)
            => (CouchOptionsBuilder<TContext>)base.UseBasicAuthentication(username, password);

        /// <summary>
        /// Enables cookie authentication. 
        /// For cookie authentication (RFC 2109) CouchDB generates a token that the client can use for the next few requests to CouchDB. Tokens are valid until a timeout.
        /// </summary>
        /// <param name="username">Server username.</param>
        /// <param name="password">Server password.</param>
        /// <param name="cookieDuration">Cookie duration in minutes.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> UseCookieAuthentication(string username, string password, int cookieDuration = 10)
            => (CouchOptionsBuilder<TContext>)base.UseCookieAuthentication(username, password, cookieDuration);

        /// <summary>
        /// Enables proxy authentication. 
        /// </summary>
        /// <param name="username">Server username.</param>
        /// <param name="roles">Server roles.</param>
        /// <param name="token">Computed authentication token.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> UseProxyAuthentication(string username, IReadOnlyCollection<string> roles, string? token = null)
            => (CouchOptionsBuilder<TContext>)base.UseProxyAuthentication(username, roles, token);

        /// <summary>
        /// Enables JWT authentication. 
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> UseJwtAuthentication(string token)
            => (CouchOptionsBuilder<TContext>)base.UseJwtAuthentication(token);

        /// <summary>
        /// Enables JWT authentication. The function is called before each call.
        /// </summary>
        /// <param name="tokenGenerator">Function that returns a JWT token asynchronous.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> UseJwtAuthentication(Func<Task<string>> tokenGenerator)
            => (CouchOptionsBuilder<TContext>)base.UseJwtAuthentication(tokenGenerator);

        /// <summary>
        /// Disables documents pluralization in requests.
        /// </summary>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> DisableDocumentPluralization()
            => (CouchOptionsBuilder<TContext>)base.DisableDocumentPluralization();

        /// <summary>
        /// Sets the format case for documents. Default: underscore_case.
        /// </summary>
        /// <param name="type">The type of case format.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> SetDocumentCase(DocumentCaseType type)
            => (CouchOptionsBuilder<TContext>)base.SetDocumentCase(type);

        /// <summary>
        /// Sets the format case for properties. Default: camelCase.
        /// </summary>
        /// <param name="type">The type of case format.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> SetPropertyCase(PropertyCaseType type)
            => (CouchOptionsBuilder<TContext>)base.SetPropertyCase(type);
        
        /// <summary>
        /// Set the field to use to identify document types. Default: <c>split_discriminator</c>.
        /// </summary>
        /// <param name="databaseSplitDiscriminator">The document field to use as discriminator.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> WithDatabaseSplitDiscriminator(string databaseSplitDiscriminator)
            => (CouchOptionsBuilder<TContext>)base.WithDatabaseSplitDiscriminator(databaseSplitDiscriminator);

        /// <summary>
        /// Sets how to handle null values during serialization.
        /// </summary>
        /// <param name="nullValueHandling">The type of null value handling.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> SetJsonNullValueHandling(NullValueHandling nullValueHandling)
            => (CouchOptionsBuilder<TContext>)base.SetJsonNullValueHandling(nullValueHandling);

        /// <summary>
        /// Disables log out on client dispose. 
        /// </summary>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> DisableLogOutOnDispose()
            => (CouchOptionsBuilder<TContext>)base.DisableLogOutOnDispose();

        /// <summary>
        /// Removes any SSL certificate validation.
        /// </summary>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> IgnoreCertificateValidation()
            => (CouchOptionsBuilder<TContext>)base.IgnoreCertificateValidation();

        /// <summary>
        /// Sets a custom SSL validation rule.
        /// </summary>
        /// <param name="serverCertificateCustomValidationCallback">SSL validation function</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> ConfigureCertificateValidation(Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>
            serverCertificateCustomValidationCallback)
            => (CouchOptionsBuilder<TContext>)base.ConfigureCertificateValidation(serverCertificateCustomValidationCallback);

        /// <summary>
        /// Configure the Flurl client.
        /// </summary>
        /// <param name="flurlSettingsAction">An action representing to configure Flurl.</param>
        /// <returns>Return the current instance to chain calls.</returns>
        public new virtual CouchOptionsBuilder<TContext> ConfigureFlurlClient(Action<ClientFlurlHttpSettings> flurlSettingsAction)
            => (CouchOptionsBuilder<TContext>)base.ConfigureFlurlClient(flurlSettingsAction);
    }
}
