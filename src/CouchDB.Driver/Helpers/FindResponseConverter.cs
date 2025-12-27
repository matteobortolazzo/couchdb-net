using System.Collections.ObjectModel;
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

        return typeToConvert.GetGenericTypeDefinition() == typeof(ReadItemResponse<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type sourceType = typeToConvert.GetGenericArguments()[0];

        Type converterType = typeof(FindResponseConverter<>).MakeGenericType(sourceType);

        return (JsonConverter)Activator.CreateInstance(converterType, options)!;
    }
}

public class FindResponseConverter<TSource> : JsonConverter<ReadItemResponse<TSource>>
    where TSource : class
{
    public override ReadItemResponse<TSource> Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        JsonElement root = jsonDoc.RootElement;

        // Extract metadata
        var rev = root.GetProperty("_rev").GetString();
        var conflicts = root.TryGetProperty("_conflicts", out JsonElement conflictsElement)
            ? conflictsElement.Deserialize<string[]>(jsonSerializerOptions)
            : null;
        var deletedConflicts = root.TryGetProperty("_deleted_conflicts", out JsonElement deletedConflictsElement)
            ? deletedConflictsElement.Deserialize<string[]>(jsonSerializerOptions)
            : null;
        var localSeq = root.TryGetProperty("_local_seq", out JsonElement localSeqElement)
            ? localSeqElement.GetInt32()
            : (int?)null;
        RevisionInfo[]? revsInfo = root.TryGetProperty("_revs_info", out JsonElement revisionInfoElement)
            ? revisionInfoElement.Deserialize<RevisionInfo[]>(jsonSerializerOptions)
            : null;
        Revisions? revisions = root.TryGetProperty("_revisions", out JsonElement revisionsElement)
            ? revisionsElement.Deserialize<Revisions>(jsonSerializerOptions)
            : null;
        ReadOnlyDictionary<string, ReadItemAttachment>? attachments =
            root.TryGetProperty("_attachments", out JsonElement attachmentElement)
                ? attachmentElement.Deserialize<ReadOnlyDictionary<string, ReadItemAttachment>>(jsonSerializerOptions)
                : null;
        if (attachments != null)
        {
            foreach ((var name, ReadItemAttachment value) in attachments)
            {
                value.Name = name;
            }
        }
        
        var deleted = root.TryGetProperty("deleted", out JsonElement deletedElement) &&
                      deletedElement.Deserialize<bool>(jsonSerializerOptions);
        TSource? document = root.Deserialize<TSource>(jsonSerializerOptions);

        return new ReadItemResponse<TSource>(
            document!,
            rev!,
            conflicts,
            deletedConflicts,
            localSeq,
            revsInfo,
            revisions,
            attachments?.Values.ToArray(),
            deleted);
    }

    public override void Write(Utf8JsonWriter writer, ReadItemResponse<TSource> value, JsonSerializerOptions jsonSerializerOptions)
    {
        throw new NotImplementedException("Writing FindResponse is not supported");
    }
}