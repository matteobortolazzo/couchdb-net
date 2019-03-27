using CouchDB.Driver.Types;
using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace CouchDB.Driver
{ 
    internal enum AuthenticationType
    {
        None, Basic, Cookie, Proxy
    }

    public class CouchSettings
    {
        internal AuthenticationType AuthenticationType { get; private set; }
        internal string Username { get; private set; }
        internal string Password { get; private set; }
        internal int CookiesDuration { get; private set; }
        internal bool PluralizeEntitis { get; private set; }
        internal EntityCaseType EntityCaseType { get; private set; }
        internal PropertyCaseType PropertiesCase { get; private set; }
        internal bool CheckDatabaseExists { get; private set; }
        public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback { get; private set; }

        internal CouchSettings()
        {
            AuthenticationType = AuthenticationType.None;
            PluralizeEntitis = true;
            EntityCaseType = (EntityCaseType)EntityCaseType.UnderscoreCase;
            PropertiesCase = PropertyCaseType.CamelCase;
        }

        public CouchSettings ConfigureBasicAuthentication(string username, string password)
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
        public CouchSettings ConfigureCookieAuthentication(string username, string password, int cookieDuration = 10)
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
        public CouchSettings IgnoreCertificateValidation()
        {
            ServerCertificateCustomValidationCallback = (m,x,c,s) => true;
            return this;
        }
        public CouchSettings ConfigureCertificateValidation(Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> 
            serverCertificateCustomValidationCallback)
        {
            ServerCertificateCustomValidationCallback = serverCertificateCustomValidationCallback ?? 
                throw new ArgumentNullException(nameof(serverCertificateCustomValidationCallback));
            return this;
        }
        public CouchSettings DisableEntitisPluralization()
        {
            PluralizeEntitis = false;
            return this;
        }
        public CouchSettings SetEntityCase(EntityCaseType type)
        {
            EntityCaseType = type;
            return this;
        }
        public CouchSettings SetPropertyCase(PropertyCaseType type)
        {
            PropertiesCase = type;
            return this;
        }
        public CouchSettings EnsureDatabaseExists()
        {
            CheckDatabaseExists = true;
            return this;
        }
    }
}
