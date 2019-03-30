using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace CouchDB.Driver.Settings
{ 
    internal enum AuthenticationType
    {
        None, Basic, Cookie, Proxy
    }

    /// <summary>
    /// A class that contains all the client settings.
    /// </summary>
    public class CouchSettings
    {
        internal AuthenticationType AuthenticationType { get; private set; }
        internal string Username { get; private set; }
        internal string Password { get; private set; }
        internal int CookiesDuration { get; private set; }
        internal bool PluralizeEntitis { get; private set; }
        internal DocumentCaseType DocumentsCaseType { get; private set; }
        internal PropertyCaseType PropertiesCase { get; private set; }
        internal bool CheckDatabaseExists { get; private set; }
        internal Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback { get; private set; }

        internal CouchSettings()
        {
            AuthenticationType = AuthenticationType.None;
            PluralizeEntitis = true;
            DocumentsCaseType = (DocumentCaseType)DocumentCaseType.UnderscoreCase;
            PropertiesCase = PropertyCaseType.CamelCase;
        }

        /// <summary>
        /// Enables basic authentication. 
        /// Basic authentication (RFC 2617) is a quick and simple way to authenticate with CouchDB. The main drawback is the need to send user credentials with each request which may be insecure and could hurt operation performance (since CouchDB must compute the password hash with every request).
        /// </summary>
        /// <param name="username">Server username.</param>
        /// <param name="password">Server password.</param>
        /// <returns>The current settings</returns>
        public CouchSettings UseBasicAuthentication(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            AuthenticationType = AuthenticationType.Basic;
            Username = username;
            Password = password;
            return this;
        }
        /// <summary>
        /// Enables cookie authentication. 
        /// For cookie authentication (RFC 2109) CouchDB generates a token that the client can use for the next few requests to CouchDB. Tokens are valid until a timeout.
        /// </summary>
        /// <param name="username">Server username.</param>
        /// <param name="password">Server password.</param>
        /// <param name="cookieDuration">Cookie duration in minutes.</param>
        /// <returns>The current settings</returns>
        public CouchSettings UseCookieAuthentication(string username, string password, int cookieDuration = 10)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));
            if (cookieDuration < 1)
                throw new ArgumentException(nameof(cookieDuration), "Cookie duration must be greater than zero.");

            AuthenticationType = AuthenticationType.Cookie;
            Username = username;
            Password = password;
            CookiesDuration = cookieDuration;
            return this;
        }
        /// <summary>
        /// Removes any SSL certificate validation.
        /// </summary>
        /// <returns>The current settings</returns>
        public CouchSettings IgnoreCertificateValidation()
        {
            ServerCertificateCustomValidationCallback = (m,x,c,s) => true;
            return this;
        }
        /// <summary>
        /// Sets a custom SSL validation rule.
        /// </summary>
        /// <param name="serverCertificateCustomValidationCallback">SSL validation function</param>
        /// <returns>The current settings</returns>
        public CouchSettings ConfigureCertificateValidation(Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> 
            serverCertificateCustomValidationCallback)
        {
            ServerCertificateCustomValidationCallback = serverCertificateCustomValidationCallback ?? 
                throw new ArgumentNullException(nameof(serverCertificateCustomValidationCallback));
            return this;
        }
        /// <summary>
        /// Disables documents pluralization in requests.
        /// </summary>
        /// <returns>The current settings</returns>
        public CouchSettings DisableDocumentPluralization()
        {
            PluralizeEntitis = false;
            return this;
        }
        /// <summary>
        /// Sets the format case for documents. Default: underscore_case.
        /// </summary>
        /// <param name="type">The type of case format.</param>
        /// <returns>The current settings</returns>
        public CouchSettings SetDocumentCase(DocumentCaseType type)
        {
            DocumentsCaseType = type;
            return this;
        }
        /// <summary>
        /// Sets the format case for properties. Default: camelCase.
        /// </summary>
        /// <param name="type">The type of case format.</param>
        /// <returns>The current settings</returns>
        public CouchSettings SetPropertyCase(PropertyCaseType type)
        {
            PropertiesCase = type;
            return this;
        }
        /// <summary>
        /// If a database doesn't exists, it creates it.
        /// </summary>
        /// <returns>The current settings</returns>
        public CouchSettings EnsureDatabaseExists()
        {
            CheckDatabaseExists = true;
            return this;
        }
    }
}
