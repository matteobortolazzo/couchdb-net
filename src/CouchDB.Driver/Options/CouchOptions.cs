using System;
using System.Text.Json;

namespace CouchDB.Driver.Options;

public abstract class CouchOptions
{
    public abstract Type ContextType { get; }
    internal Uri? Endpoint { get; set; }
    internal bool CheckDatabaseExists { get; set; }
    internal bool OverrideExistingIndexes { get; set; }
    internal ICouchAuthentication? Authentication { get; set; }
    internal JsonSerializerOptions? JsonSerializerOptions { get; set; }
    internal string? DatabaseSplitDiscriminator { get; set; }
    internal bool LogOutOnDispose { get; set; } = true;
    internal bool ThrowOnQueryWarning { get; set; }
}