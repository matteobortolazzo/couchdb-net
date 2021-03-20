using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CouchDB.Driver.Helpers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
    internal class MicrosecondEpochConverter : DateTimeConverterBase
    {
        private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(((DateTime)value - Epoch).TotalMilliseconds + "000");
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Check.NotNull(reader, nameof(reader));

            return reader.Value != null ?                 
                Epoch.AddMilliseconds((long)reader.Value / 1000d) as object : 
                null;
        }
    }
}