using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

/// <summary>
/// Represents a CouchDB document info.
/// </summary>
[Serializable]
public class CouchDocumentInfo
{
    [DataMember]
    [JsonPropertyName("id")]
    public string Id { get; private set; }

    [DataMember]
    [JsonPropertyName("key")]
    public string Key { get; private set; }

    [DataMember]
    [JsonPropertyName("value")]
    private CouchDocumentInfoValue Value { get; set; }

    public string Rev => Value.Rev;

    private class CouchDocumentInfoValue
    {
        [JsonPropertyName("rev")]
        public string Rev { get; set; }
    }
}