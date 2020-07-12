using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CouchDB.Driver.Helpers;
using Flurl.Http.Configuration;

namespace CouchDB.Driver.Settings
{
    /// <summary>
    /// A class that contains all the client settings.
    /// </summary>
    internal class CouchSettings: ICouchContextConfigurator
    {
        public AuthenticationType AuthenticationType { get; private set; }
        public string? Username { get; private set; }
        public IReadOnlyCollection<string>? Roles { get; private set; }
        public string? Password { get; private set; }
        public int CookiesDuration { get; private set; }
        public Func<Task<string>>? JwtTokenGenerator { get; private set; }
        public bool PluralizeEntities { get; private set; }
        public DocumentCaseType DocumentsCaseType { get; private set; }
        public PropertyCaseType PropertiesCase { get; private set; }
        public bool LogOutOnDispose { get; private set; }
        public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>? ServerCertificateCustomValidationCallback { get; private set; }
        public Action<ClientFlurlHttpSettings>? FlurlSettingsAction { get; private set; }
        public Uri Endpoint { get; set; } 
        public bool CheckDatabaseExists { get; private set; } 

        public CouchSettings()
        {
            Endpoint = new Uri("http://localhost:5984/");
            AuthenticationType = AuthenticationType.None;
            PluralizeEntities = true;
            DocumentsCaseType = DocumentCaseType.UnderscoreCase;
            PropertiesCase = PropertyCaseType.CamelCase;
            LogOutOnDispose = true;
        }

        public ICouchConfigurator UseBasicAuthentication(string username, string password)
        {
            Check.NotNull(username, nameof(username));
            Check.NotNull(password, nameof(password));

            AuthenticationType = AuthenticationType.Basic;
            Username = username;
            Password = password;
            return this;
        }

        public ICouchConfigurator UseCookieAuthentication(string username, string password, int cookieDuration = 10)
        {
            Check.NotNull(username, nameof(username));
            Check.NotNull(password, nameof(password));

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

        public ICouchConfigurator UseProxyAuthentication(string username, IReadOnlyCollection<string> roles, string? token = null)
        {
            Check.NotNull(username, nameof(username));
            Check.NotNull(roles, nameof(roles));

            AuthenticationType = AuthenticationType.Proxy;
            Username = username;
            Roles = roles;
            Password = token;
            return this;
        }

        public ICouchConfigurator UseJwtAuthentication(string token)
        {
            return UseJwtAuthentication(() => Task.FromResult(token));
        }

        public ICouchConfigurator UseJwtAuthentication(Func<Task<string>> tokenGenerator)
        {
            AuthenticationType = AuthenticationType.Jwt;
            JwtTokenGenerator = tokenGenerator;
            return this;
        }

        public ICouchConfigurator IgnoreCertificateValidation()
        {
            ServerCertificateCustomValidationCallback = (m,x,c,s) => true;
            return this;
        }

        public ICouchConfigurator ConfigureCertificateValidation(Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> 
            serverCertificateCustomValidationCallback)
        {
            Check.NotNull(serverCertificateCustomValidationCallback, nameof(serverCertificateCustomValidationCallback));
            ServerCertificateCustomValidationCallback = serverCertificateCustomValidationCallback;
            return this;
        }

        public ICouchConfigurator DisableDocumentPluralization()
        {
            PluralizeEntities = false;
            return this;
        }

        public ICouchConfigurator SetDocumentCase(DocumentCaseType type)
        {
            DocumentsCaseType = type;
            return this;
        }

        public ICouchConfigurator SetPropertyCase(PropertyCaseType type)
        {
            PropertiesCase = type;
            return this;
        }

        public ICouchConfigurator DisableLogOutOnDispose()
        {
            LogOutOnDispose = false;
            return this;
        }

        public ICouchConfigurator ConfigureFlurlClient(Action<ClientFlurlHttpSettings> flurlSettingsAction)
        {
            FlurlSettingsAction = flurlSettingsAction;
            return this;
        }

        public ICouchContextConfigurator UseEndpoint(string endpoint)
        {
            Endpoint = new Uri(endpoint);
            return this;
        }

        public ICouchContextConfigurator UseEndpoint(Uri endpointUri)
        {
            Endpoint = endpointUri;
            return this;
        }

        public ICouchContextConfigurator EnsureDatabaseExists()
        {
            CheckDatabaseExists = true;
            return this;
        }
    }
}
