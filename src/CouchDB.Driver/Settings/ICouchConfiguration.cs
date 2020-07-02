using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CouchDB.Driver.Settings
{
    /// <summary>
    /// Let setup
    /// </summary>
    public interface ICouchConfiguration
    {
        /// <summary>
        /// Enables basic authentication. 
        /// Basic authentication (RFC 2617) is a quick and simple way to authenticate with CouchDB. The main drawback is the need to send user credentials with each request which may be insecure and could hurt operation performance (since CouchDB must compute the password hash with every request).
        /// </summary>
        /// <param name="username">Server username.</param>
        /// <param name="password">Server password.</param>
        /// <returns>The current settings</returns>
        ICouchConfiguration UseBasicAuthentication(string username, string password);

        /// <summary>
        /// Enables cookie authentication. 
        /// For cookie authentication (RFC 2109) CouchDB generates a token that the client can use for the next few requests to CouchDB. Tokens are valid until a timeout.
        /// </summary>
        /// <param name="username">Server username.</param>
        /// <param name="password">Server password.</param>
        /// <param name="cookieDuration">Cookie duration in minutes.</param>
        /// <returns>The current settings</returns>
        ICouchConfiguration UseCookieAuthentication(string username, string password, int cookieDuration = 10);

        /// <summary>
        /// Enables proxy authentication. 
        /// </summary>
        /// <param name="username">Server username.</param>
        /// <param name="roles">Server roles.</param>
        /// <param name="token">Computed authentication token.</param>
        /// <returns>The current settings</returns>
        ICouchConfiguration UseProxyAuthentication(string username, IReadOnlyCollection<string> roles, string? token = null);

        /// <summary>
        /// Enables JWT authentication. 
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>The current settings</returns>
        ICouchConfiguration UseJwtAuthentication(string token);

        /// <summary>
        /// Enables JWT authentication. The function is called before each call.
        /// </summary>
        /// <param name="tokenGenerator">Function that returns a JWT token asynchronous.</param>
        /// <returns>The current settings</returns>
        ICouchConfiguration UseJwtAuthentication(Func<Task<string>> tokenGenerator);

        /// <summary>
        /// Removes any SSL certificate validation.
        /// </summary>
        /// <returns>The current settings</returns>
        ICouchConfiguration IgnoreCertificateValidation();

        /// <summary>
        /// Sets a custom SSL validation rule.
        /// </summary>
        /// <param name="serverCertificateCustomValidationCallback">SSL validation function</param>
        /// <returns>The current settings</returns>
        ICouchConfiguration ConfigureCertificateValidation(
            Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>
                serverCertificateCustomValidationCallback);

        /// <summary>
        /// Disables documents pluralization in requests.
        /// </summary>
        /// <returns>The current settings</returns>
        ICouchConfiguration DisableDocumentPluralization();

        /// <summary>
        /// Sets the format case for documents. Default: underscore_case.
        /// </summary>
        /// <param name="type">The type of case format.</param>
        /// <returns>The current settings</returns>
        ICouchConfiguration SetDocumentCase(DocumentCaseType type);

        /// <summary>
        /// Sets the format case for properties. Default: camelCase.
        /// </summary>
        /// <param name="type">The type of case format.</param>
        /// <returns>The current settings</returns>
        ICouchConfiguration SetPropertyCase(PropertyCaseType type);

        /// <summary>
        /// Disables log out on client dispose. 
        /// </summary>
        /// <returns>The current settings</returns>
        ICouchConfiguration DisableLogOutOnDispose();
    }
}