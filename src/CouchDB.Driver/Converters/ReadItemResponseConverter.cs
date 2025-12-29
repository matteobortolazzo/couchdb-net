using System.Buffers;
using System.Text.Json;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Converters;

public class ReadItemResponseConverterFactory : JsonConverterFactory
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

        Type converterType = typeof(ReadItemResponseConverter<>).MakeGenericType(sourceType);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class ReadItemResponseConverter<TSource> : JsonConverter<ReadItemResponse<TSource>>
    where TSource : class
{
    public override ReadItemResponse<TSource> Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        var bufferWriter = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(bufferWriter);

        string? rev = null;
        string[]? conflicts = null;
        string[]? deletedConflicts = null;
        int? localSeq = null;
        RevisionInfo[]? revsInfo = null;
        Revisions? revisions = null;
        var deleted = false;
        Dictionary<string, ReadItemAttachment>? attachments = null;

        writer.WriteStartObject();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected property name");
            }

            var propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case "_rev":
                    rev = reader.GetString();
                    writer.WritePropertyName(GetPropertyName("rev", jsonSerializerOptions));
                    writer.WriteStringValue(rev);
                    break;
                case "_id":
                    writer.WritePropertyName(GetPropertyName("id", jsonSerializerOptions));
                    JsonSerializer.Serialize(writer, JsonSerializer.Deserialize<JsonElement>(ref reader),
                        jsonSerializerOptions);
                    break;
                case "_conflicts":
                    conflicts = JsonSerializer.Deserialize<string[]>(ref reader, jsonSerializerOptions);
                    break;
                case "_deleted_conflicts":
                    deletedConflicts = JsonSerializer.Deserialize<string[]>(ref reader, jsonSerializerOptions);
                    break;
                case "_local_seq":
                    localSeq = reader.GetInt32();
                    break;
                case "_revs_info":
                    revsInfo = JsonSerializer.Deserialize<RevisionInfo[]>(ref reader, jsonSerializerOptions);
                    break;
                case "_revisions":
                    revisions = JsonSerializer.Deserialize<Revisions>(ref reader, jsonSerializerOptions);
                    break;
                case "_deleted":
                    deleted = reader.GetBoolean();
                    break;
                case "_attachments":
                    attachments =
                        JsonSerializer.Deserialize<Dictionary<string, ReadItemAttachment>>(ref reader,
                            jsonSerializerOptions);
                    if (attachments != null)
                    {
                        foreach ((var name, ReadItemAttachment value) in attachments)
                        {
                            value.Name = name;
                        }
                    }

                    break;
                default:
                    writer.WritePropertyName(propertyName);
                    JsonSerializer.Serialize(writer, JsonSerializer.Deserialize<JsonElement>(ref reader),
                        jsonSerializerOptions);
                    break;
            }
        }

        writer.WriteEndObject();
        writer.Flush();

        var jsonReader = new Utf8JsonReader(bufferWriter.WrittenSpan);
        TSource? document = JsonSerializer.Deserialize<TSource>(ref jsonReader, jsonSerializerOptions);

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

    public override void Write(Utf8JsonWriter writer, ReadItemResponse<TSource> value,
        JsonSerializerOptions jsonSerializerOptions)
    {
        throw new NotImplementedException("Writing FindResponse is not supported");
    }

    private static string GetPropertyName(string name, JsonSerializerOptions options)
    {
        return options.PropertyNamingPolicy?.ConvertName(name) ?? name;
    }
}