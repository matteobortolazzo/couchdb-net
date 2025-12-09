using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Converters;

internal class AttachmentsParsedConverter : JsonConverter<Dictionary<string, CouchAttachment>>
{
    public override void Write(Utf8JsonWriter writer, Dictionary<string, CouchAttachment>? value,
        JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var filtered = value
            .Where(kvp => kvp.Value.FileInfo is null)
            .ToDictionary(k => k.Key, v => v.Value);

        JsonSerializer.Serialize(writer, filtered, options);
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(Dictionary<string, CouchAttachment>);
    }

    public override Dictionary<string, CouchAttachment> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}