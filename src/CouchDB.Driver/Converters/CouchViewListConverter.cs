using System.Text.Json;
using CouchDB.Driver.Views;

namespace CouchDB.Driver.Converters;

internal class CouchViewListConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        Type genericType = typeToConvert.GetGenericTypeDefinition();
        return genericType == typeof(CouchViewList<,,>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type[] typeArgs = typeToConvert.GetGenericArguments();
        Type converterType = typeof(CouchViewListConverter<,,>)
            .MakeGenericType(typeArgs[0], typeArgs[1], typeArgs[2]);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

internal class CouchViewListConverter<TKey, TValue, TDoc> : JsonConverter<CouchViewList<TKey, TValue, TDoc>>
    where TDoc : class
{
    public override CouchViewList<TKey, TValue, TDoc>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var totalRows = 0;
        var offset = 0;
        CouchView<TKey, TValue, TDoc>[]? rows = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new CouchViewList<TKey, TValue, TDoc>(
                    rows ?? [],
                    totalRows,
                    offset);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "total_rows":
                    totalRows = reader.GetInt32();
                    break;
                case "offset":
                    offset = reader.GetInt32();
                    break;
                case "rows":
                    rows = JsonSerializer.Deserialize<CouchView<TKey, TValue, TDoc>[]>(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException();
    }

    public override void Write(
        Utf8JsonWriter writer,
        CouchViewList<TKey, TValue, TDoc> value,
        JsonSerializerOptions options)
    {
        throw new NotSupportedException("Writing CouchViewList is not supported.");
    }
}