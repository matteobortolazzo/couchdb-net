using System;

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Helpers;

public class CouchDocumentConverter : JsonConverter<CouchDocument>
{
    private readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> _optionsCache = new();

    public override CouchDocument? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonSerializerOptions optionsWithoutConverter = _optionsCache.GetValue(options, CreateOptionsWithoutConverter);
        // using var docx = JsonDocument.ParseValue(ref reader);
        // string jsonString = docx.RootElement.GetRawText();
        var doc = (CouchDocument?)JsonSerializer.Deserialize(ref reader, typeToConvert, optionsWithoutConverter);
        if (doc is { AttachmentsParsed.Count: > 0 })
        {
            doc.Attachments = new CouchAttachmentsCollection(doc.AttachmentsParsed);
        }

        return doc;
    }

    public override void Write(Utf8JsonWriter writer, CouchDocument value, JsonSerializerOptions options)
    {
        JsonSerializerOptions optionsWithoutConverter = _optionsCache.GetValue(options, CreateOptionsWithoutConverter);
        JsonSerializer.Serialize(writer, value, value.GetType(), optionsWithoutConverter);
    }

    private static JsonSerializerOptions CreateOptionsWithoutConverter(JsonSerializerOptions options)
    {
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Remove(newOptions.Converters.First(c => c is CouchDocumentConverter));
        return newOptions;
    }

    public override bool CanConvert(Type typeToConvert) =>
        typeof(CouchDocument).IsAssignableFrom(typeToConvert);
}