using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CouchDB.Driver.Extensions;

namespace CouchDB.Driver.Helpers;

public class CouchJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    private readonly string? _databaseSplitDiscriminator;

    internal CouchJsonTypeInfoResolver(string? databaseSplitDiscriminator)
    {
        _databaseSplitDiscriminator = databaseSplitDiscriminator;
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(type);

        JsonTypeInfo typeInfo = base.GetTypeInfo(type, options);

        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return typeInfo;
        }

        JsonNamingPolicy jsonNamingPolicy = options.PropertyNamingPolicy ?? JsonNamingPolicy.CamelCase;

        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            if (property.DeclaringType.Assembly != GetType().Assembly)
            {
                property.Name = type.GetProperty(property.Name)?.GetCouchPropertyName(jsonNamingPolicy) ??
                                property.Name;
            }

            if (property.Name == CouchClient.DefaultDatabaseSplitDiscriminator &&
                !string.IsNullOrWhiteSpace(_databaseSplitDiscriminator))
            {
                property.Name = _databaseSplitDiscriminator;
            }
        }

        return typeInfo;
    }
}