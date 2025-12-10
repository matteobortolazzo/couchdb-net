using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Helpers;

internal class MicrosecondEpochConverter : JsonConverter<DateTimeOffset>
{
    private static readonly DateTimeOffset Epoch = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        var microseconds = (long)((value - Epoch).TotalMilliseconds * 1000);
        writer.WriteNumberValue(microseconds);
    }

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var microseconds = reader.GetInt64();
            return Epoch.AddMilliseconds(microseconds / 1000d);
        }

        throw new JsonException("Expected number token for DateTimeOffset conversion.");
    }
}