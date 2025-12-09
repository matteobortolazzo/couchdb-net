using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Helpers;

internal class MicrosecondEpochConverter : JsonConverter<DateTime>
{
    private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var microseconds = (long)((value - Epoch).TotalMilliseconds * 1000);
        writer.WriteNumberValue(microseconds);
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var microseconds = reader.GetInt64();
            return Epoch.AddMilliseconds(microseconds / 1000d);
        }

        throw new JsonException("Expected number token for DateTime conversion.");
    }
}