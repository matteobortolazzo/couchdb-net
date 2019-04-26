using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
namespace CouchDB.Driver.Helpers
{
    internal class MicrosecondEpochConverter : DateTimeConverterBase
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(((DateTime)value - _epoch).TotalMilliseconds + "000");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value != null ?                 
                (object)_epoch.AddMilliseconds((long)reader.Value / 1000d) : 
                null;
        }
    }
}
#pragma warning restore CA1812 // Avoid uninstantiated internal classes