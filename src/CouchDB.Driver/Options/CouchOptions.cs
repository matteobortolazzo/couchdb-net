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
    public abstract class CouchOptions
    {
        public abstract Type ContextType { get; }

        internal Uri? Endpoint { get; set; }
        internal bool CheckDatabaseExists { get; set; }
        internal bool OverrideExistingIndexes { get; set; }

        internal AuthenticationType AuthenticationType { get; set; }
        internal string? Username { get; set; }
        internal string? Password { get; set; }
        internal IReadOnlyCollection<string>? Roles { get; set; }
        internal int CookiesDuration { get; set; }
        internal Func<Task<string>>? JwtTokenGenerator { get; set; }

        internal bool PluralizeEntities { get; set; }
        internal DocumentCaseType DocumentsCaseType { get; set; }
        internal PropertyCaseType PropertiesCase { get; set; }
        
        internal string? DatabaseSplitDiscriminator { get; set; }
        internal NullValueHandling? NullValueHandling { get; set; }
        internal bool LogOutOnDispose { get; set; }

        internal Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>? ServerCertificateCustomValidationCallback { get; set; }
        internal Action<ClientFlurlHttpSettings>? ClientFlurlHttpSettingsAction { get; set; }

        internal CouchOptions()
        {
            AuthenticationType = AuthenticationType.None;
            PluralizeEntities = true;
            DocumentsCaseType = DocumentCaseType.UnderscoreCase;
            PropertiesCase = PropertyCaseType.CamelCase;
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            LogOutOnDispose = true;
        }
    }
}