using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace CouchDB.Driver.Helpers
{
    internal class MicrosecondEpochConverter : DateTimeConverterBase
    {
        private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            
            writer.WriteRawValue(((DateTime)value - Epoch).TotalMilliseconds + "000");
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            Check.NotNull(reader, nameof(reader));

            return reader.Value != null ?                 
                Epoch.AddMilliseconds((long)reader.Value / 1000d) : 
                null;
        }
    }
}