using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

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
        /// If a database doesn't exists, it creates it.
        /// </summary>
        /// <returns>The current settings</returns>
        ICouchConfiguration EnsureDatabaseExists();

        /// <summary>
        /// Disables log out on client dispose. 
        /// </summary>
        /// <returns>The current settings</returns>
        ICouchConfiguration DisableLogOutOnDispose();
    }

    internal enum AuthenticationType
    {
        None, Basic, Cookie, Proxy
    }

    /// <summary>
    /// A class that contains all the client settings.
    /// </summary>
    internal class CouchSettings: ICouchConfiguration
    {
        public AuthenticationType AuthenticationType { get; private set; }
        public string? Username { get; private set; }
        public IReadOnlyCollection<string>? Roles { get; private set; }
        public string? Password { get; private set; }
        public int CookiesDuration { get; private set; }
        public bool PluralizeEntities { get; private set; }
        public DocumentCaseType DocumentsCaseType { get; private set; }
        public PropertyCaseType PropertiesCase { get; private set; }
        public bool CheckDatabaseExists { get; private set; }
        public bool LogOutOnDispose { get; private set; }
        public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>? ServerCertificateCustomValidationCallback { get; private set; }

        public CouchSettings()
        {
            AuthenticationType = AuthenticationType.None;
            PluralizeEntities = true;
            DocumentsCaseType = DocumentCaseType.UnderscoreCase;
            PropertiesCase = PropertyCaseType.CamelCase;
            LogOutOnDispose = true;
        }

        public ICouchConfiguration UseBasicAuthentication(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            AuthenticationType = AuthenticationType.Basic;
            Username = username;
            Password = password;
            return this;
        }

        public ICouchConfiguration UseCookieAuthentication(string username, string password, int cookieDuration = 10)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }
            if (cookieDuration < 1)
            {
                throw new ArgumentException("Cookie duration must be greater than zero.", nameof(cookieDuration));
            }

            AuthenticationType = AuthenticationType.Cookie;
            Username = username;
            Password = password;
            CookiesDuration = cookieDuration;
            return this;
        }

        public ICouchConfiguration UseProxyAuthentication(string username, IReadOnlyCollection<string> roles, string? token = null)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            AuthenticationType = AuthenticationType.Proxy;
            Username = username;
            Roles = roles ?? throw new ArgumentNullException(nameof(roles));
            Password = token;
            return this;
        }

        public ICouchConfiguration IgnoreCertificateValidation()
        {
            ServerCertificateCustomValidationCallback = (m,x,c,s) => true;
            return this;
        }

        public ICouchConfiguration ConfigureCertificateValidation(Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> 
            serverCertificateCustomValidationCallback)
        {
            ServerCertificateCustomValidationCallback = serverCertificateCustomValidationCallback ?? 
                throw new ArgumentNullException(nameof(serverCertificateCustomValidationCallback));
            return this;
        }

        public ICouchConfiguration DisableDocumentPluralization()
        {
            PluralizeEntities = false;
            return this;
        }

        public ICouchConfiguration SetDocumentCase(DocumentCaseType type)
        {
            DocumentsCaseType = type;
            return this;
        }

        public ICouchConfiguration SetPropertyCase(PropertyCaseType type)
        {
            PropertiesCase = type;
            return this;
        }

        public ICouchConfiguration EnsureDatabaseExists()
        {
            CheckDatabaseExists = true;
            return this;
        }

        public ICouchConfiguration DisableLogOutOnDispose()
        {
            LogOutOnDispose = false;
            return this;
        }
    }
}
