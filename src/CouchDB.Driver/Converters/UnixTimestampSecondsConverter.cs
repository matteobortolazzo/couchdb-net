using System.Text.Json;

namespace CouchDB.Driver.Converters;

internal class UnixTimestampSecondsConverter : JsonConverter<DateTimeOffset>
{
    private static readonly DateTimeOffset Epoch = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        var seconds = (long)(value.ToUniversalTime() - Epoch).TotalSeconds;
        writer.WriteNumberValue(seconds);
    }

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return default;
            case JsonTokenType.Number:
                var seconds = reader.GetInt64();
                return Epoch.AddSeconds(seconds);
            case JsonTokenType.String:
                var str = reader.GetString();
                return long.TryParse(str, out seconds)
                    ? Epoch.AddSeconds(seconds)
                    : throw new JsonException("Expected integer Unix timestamp in seconds.");
            default:
                throw new JsonException("Expected number or string token for Unix timestamp conversion.");
        }
    }
}
