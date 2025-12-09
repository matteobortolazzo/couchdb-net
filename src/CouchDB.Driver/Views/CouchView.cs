using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Views;

/// <summary>
/// Base class for a view.
/// </summary>
/// <typeparam name="TKey">The type of the key</typeparam>
/// <typeparam name="TValue">The type of the value</typeparam>
/// <typeparam name="TDoc">The type of the document.</typeparam>
[Serializable]
public sealed class CouchView<TKey, TValue, TDoc>
{
    /// <summary>
    /// The document ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// The view key.
    /// </summary>
    [JsonPropertyName("key")]
    public TKey Key { get; set; }

    /// <summary>
    /// The view key.
    /// </summary>
    [JsonPropertyName("value")]
    public TValue Value { get; set; }

    /// <summary>
    /// The document.
    /// </summary>
    [JsonPropertyName("doc")]
    public TDoc Document { get; set; }
}