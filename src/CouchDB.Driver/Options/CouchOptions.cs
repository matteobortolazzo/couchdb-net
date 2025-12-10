using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CouchDB.Driver.Options;

public abstract class CouchOptions
{
    public abstract Type ContextType { get; }

    internal Uri? Endpoint { get; init; }
    internal bool CheckDatabaseExists { get; init; }
    internal bool OverrideExistingIndexes { get; init; }

    internal AuthenticationType AuthenticationType { get; init; }
    internal string? Username { get; init; }
    internal string? Password { get; init; }
    internal IReadOnlyCollection<string>? Roles { get; init; }
    internal int CookiesDuration { get; init; }
    internal Func<Task<string>>? JwtTokenGenerator { get; init; }

    internal bool PluralizeEntities { get; init; }
    internal DocumentCaseType DocumentsCaseType { get; init; }
    internal PropertyCaseType PropertiesCase { get; init; }

    internal string? DatabaseSplitDiscriminator { get; init; }
    internal JsonIgnoreCondition? JsonIgnoreCondition { get; init; }
    internal bool LogOutOnDispose { get; init; }

    internal Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>?
        ServerCertificateCustomValidationCallback { get; init; }

    internal bool ThrowOnQueryWarning { get; init; }

    internal CouchOptions()
    {
        AuthenticationType = AuthenticationType.None;
        PluralizeEntities = true;
        DocumentsCaseType = DocumentCaseType.UnderscoreCase;
        PropertiesCase = PropertyCaseType.CamelCase;
        JsonIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        LogOutOnDispose = true;
    }
}