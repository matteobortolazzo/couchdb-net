using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Flurl.Http.Configuration;

namespace CouchDB.Driver.Settings
{
    /// <summary>
    /// Configure how the driver behave
    /// </summary>
    public interface ICouchConfigurator
    {
        /// <summary>
        /// Enables basic authentication. 
        /// Basic authentication (RFC 2617) is a quick and simple way to authenticate with CouchDB. The main drawback is the need to send user credentials with each request which may be insecure and could hurt operation performance (since CouchDB must compute the password hash with every request).
        /// </summary>
        /// <param name="username">Server username.</param>
        /// <param name="password">Server password.</param>
        /// <returns>The instance to chain settings.</returns>
        ICouchConfigurator UseBasicAuthentication(string username, string password);

        /// <summary>
        /// Enables cookie authentication. 
        /// For cookie authentication (RFC 2109) CouchDB generates a token that the client can use for the next few requests to CouchDB. Tokens are valid until a timeout.
        /// </summary>
        /// <param name="username">Server username.</param>
        /// <param name="password">Server password.</param>
        /// <param name="cookieDuration">Cookie duration in minutes.</param>
        /// <returns>The instance to chain settings.</returns>
        ICouchConfigurator UseCookieAuthentication(string username, string password, int cookieDuration = 10);

        /// <summary>
        /// Enables proxy authentication. 
        /// </summary>
        /// <param name="username">Server username.</param>
        /// <param name="roles">Server roles.</param>
        /// <param name="token">Computed authentication token.</param>
        /// <returns>The instance to chain settings.</returns>
        ICouchConfigurator UseProxyAuthentication(string username, IReadOnlyCollection<string> roles, string? token = null);

        /// <summary>
        /// Enables JWT authentication. 
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>The instance to chain settings.</returns>
        ICouchConfigurator UseJwtAuthentication(string token);

        /// <summary>
        /// Enables JWT authentication. The function is called before each call.
        /// </summary>
        /// <param name="tokenGenerator">Function that returns a JWT token asynchronous.</param>
        /// <returns>The instance to chain settings.</returns>
        ICouchConfigurator UseJwtAuthentication(Func<Task<string>> tokenGenerator);

        /// <summary>
        /// Removes any SSL certificate validation.
        /// </summary>
        /// <returns>The instance to chain settings.</returns>
        ICouchConfigurator IgnoreCertificateValidation();

        /// <summary>
        /// Sets a custom SSL validation rule.
        /// </summary>
        /// <param name="serverCertificateCustomValidationCallback">SSL validation function</param>
        /// <returns>The instance to chain settings.</returns>
        ICouchConfigurator ConfigureCertificateValidation(
            Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>
                serverCertificateCustomValidationCallback);

        /// <summary>
        /// Disables documents pluralization in requests.
        /// </summary>
        /// <returns>The instance to chain settings.</returns>
        ICouchConfigurator DisableDocumentPluralization();

        /// <summary>
        /// Sets the format case for documents. Default: underscore_case.
        /// </summary>
        /// <param name="type">The type of case format.</param>
        /// <returns>The instance to chain settings.</returns>
        ICouchConfigurator SetDocumentCase(DocumentCaseType type);

        /// <summary>
        /// Sets the format case for properties. Default: camelCase.
        /// </summary>
        /// <param name="type">The type of case format.</param>
        /// <returns>The instance to chain settings.</returns>
        ICouchConfigurator SetPropertyCase(PropertyCaseType type);

        /// <summary>
        /// Disables log out on client dispose. 
        /// </summary>
        /// <returns>The instance to chain settings.</returns>
        ICouchConfigurator DisableLogOutOnDispose();

        /// <summary>
        /// Configure the Flurl client.
        /// </summary>
        /// <param name="flurlSettingsAction">An action representing to configure Flurl.</param>
        /// <returns>The instance to chain settings.</returns>
        ICouchConfigurator ConfigureFlurlClient(Action<ClientFlurlHttpSettings> flurlSettingsAction);
    }
}