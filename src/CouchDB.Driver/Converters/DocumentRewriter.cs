using System.Buffers;
using System.Text.Json;

namespace CouchDB.Driver.Converters;

internal static class DocumentRewriter
{
    internal static TDocument RewriteDocument<TDocument>(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        var bufferWriter = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(bufferWriter);

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
                case "_id":
                    WriteTransformedProperty(writer, "id", reader.GetString()!, options);
                    break;
                case "_rev":
                    WriteTransformedProperty(writer, "rev", reader.GetString()!, options);
                    break;
                default:
                    writer.WritePropertyName(propertyName);
                    JsonSerializer.Serialize(writer, JsonSerializer.Deserialize<JsonElement>(ref reader), options);
                    break;
            }
        }

        writer.WriteEndObject();
        writer.Flush();

        var jsonReader = new Utf8JsonReader(bufferWriter.WrittenSpan);
        return JsonSerializer.Deserialize<TDocument>(ref jsonReader, options)!;
    }

    internal static void WriteTransformedProperty(Utf8JsonWriter writer, string name, string value,
        JsonSerializerOptions options)
    {
        if (options.PropertyNamingPolicy == null)
        {
            writer.WritePropertyName(name);
            writer.WriteStringValue(value);
            writer.WritePropertyName(char.ToUpper(name[0]) + name[1..]);
            writer.WriteStringValue(value);
        }
        else
        {
            writer.WritePropertyName(options.PropertyNamingPolicy.ConvertName(name));
            writer.WriteStringValue(value);
        }
    }
}
