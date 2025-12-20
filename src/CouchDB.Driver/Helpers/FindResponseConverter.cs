using System.Text.Json;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Helpers;

public class FindResponseConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        return typeToConvert.GetGenericTypeDefinition() == typeof(FindResponse<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type sourceType = typeToConvert.GetGenericArguments()[0];

        Type converterType = typeof(FindResponseConverter<>).MakeGenericType(sourceType);

        return (JsonConverter)Activator.CreateInstance(converterType, options)!;
    }
}

public class FindResponseConverter<TSource>(JsonSerializerOptions options) : JsonConverter<FindResponse<TSource>>
    where TSource : class
{
    public override FindResponse<TSource>? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options1)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        JsonElement root = jsonDoc.RootElement;

        // Extract metadata
        var rev = root.GetProperty("_rev").GetString();
        var conflicts = root.TryGetProperty("_conflicts", out JsonElement c) ? c.Deserialize<string[]>(options) : null;
        var deletedConflicts = root.TryGetProperty("_deleted_conflicts", out JsonElement dc)
            ? dc.Deserialize<string[]>(options)
            : null;
        var localSeq = root.TryGetProperty("_local_seq", out JsonElement ls) ? ls.GetInt32() : (int?)null;
        RevisionInfo[]? revsInfo = root.TryGetProperty("_revs_info", out JsonElement ri)
            ? ri.Deserialize<RevisionInfo[]>(options)
            : null;
        Revisions? revisions = root.TryGetProperty("_revisions", out JsonElement r)
            ? r.Deserialize<Revisions>(options)
            : null;
        TSource? document = root.Deserialize<TSource>(options);

        return new FindResponse<TSource>(
            document!,
            rev!,
            conflicts,
            deletedConflicts,
            localSeq,
            revsInfo,
            revisions
        );
    }

    public override void Write(Utf8JsonWriter writer, FindResponse<TSource> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Writing FindResponse is not supported");
    }
}