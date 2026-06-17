using System.Text.Json;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Converters;

internal class FindResultConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        return typeToConvert.GetGenericTypeDefinition() == typeof(FindResult<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type itemType = typeToConvert.GetGenericArguments()[0];
        Type converterType = typeof(FindResultConverter<>).MakeGenericType(itemType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

internal class FindResultConverter<TItem> : JsonConverter<FindResult<TItem>>
{
    public override FindResult<TItem> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        TItem[]? docs = null;
        string? bookmark = null;
        ExecutionStats? executionStats = null;
        string? warning = null;

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

            var propName = reader.GetString()!;
            reader.Read();

            switch (propName)
            {
                case "docs":
                    docs = ReadDocs(ref reader, options);
                    break;
                case "bookmark":
                    bookmark = reader.GetString();
                    break;
                case "execution_stats":
                    executionStats = JsonSerializer.Deserialize<ExecutionStats>(ref reader, options);
                    break;
                case "warning":
                    warning = reader.GetString();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return new FindResult<TItem>(docs ?? [], bookmark ?? string.Empty, executionStats, warning);
    }

    public override void Write(Utf8JsonWriter writer, FindResult<TItem> value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Writing FindResult is not supported");
    }

    private static TItem[] ReadDocs(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array");
        }

        var isScalarProjection = IsScalarProjection(typeof(TItem));

        var items = new List<TItem>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            items.Add(isScalarProjection
                ? ReadScalarDocument(ref reader, options)
                : DocumentRewriter.RewriteDocument<TItem>(ref reader, options));
        }

        return items.ToArray();
    }

    /// <summary>
    /// Reads a scalar projection such as <c>Select(x =&gt; x.Id)</c>. The server returns each
    /// document as an object with a single field (e.g. <c>{"_id":"abc"}</c>); the value of that
    /// field is deserialized into <typeparamref name="TItem"/>, ignoring the property name so
    /// that <c>_id</c>, <c>_rev</c> and any other selected field are handled uniformly.
    /// </summary>
    private static TItem ReadScalarDocument(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        TItem result = default!;
        var found = false;

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

            reader.Read();

            if (found)
            {
                reader.Skip();
                continue;
            }

            result = JsonSerializer.Deserialize<TItem>(ref reader, options)!;
            found = true;
        }

        return result;
    }

    private static bool IsScalarProjection(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type == typeof(string)
               || type.IsPrimitive
               || type.IsEnum
               || type == typeof(Guid)
               || type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(decimal)
               || type == typeof(TimeSpan);
    }
}
