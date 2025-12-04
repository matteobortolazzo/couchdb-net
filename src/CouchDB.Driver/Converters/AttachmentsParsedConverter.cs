using System;
using System.Collections.Generic;
using System.Linq;
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Converters
{
    internal class AttachmentsParsedConverter : JsonConverter<Dictionary<string, CouchAttachment>>
    {
        public override void WriteJson(JsonWriter writer, Dictionary<string, CouchAttachment>? value,
            JsonSerializer serializer)
        {
            if (value == null)
            {
                return;
            }
            
            serializer.Serialize(writer, value
                .Where(kvp => kvp.Value.FileInfo is null)
                .ToDictionary(k => k.Key, v => v.Value)
            );
        }

        public override bool CanRead => false;

        public override Dictionary<string, CouchAttachment> ReadJson(JsonReader reader, Type objectType,
            Dictionary<string, CouchAttachment>? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}