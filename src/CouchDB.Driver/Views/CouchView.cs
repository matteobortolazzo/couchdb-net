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
    [property:JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// The view key.
    /// </summary>
    [property:JsonPropertyName("key")]
    public required TKey Key { get; init; }

    /// <summary>
    /// The view key.
    /// </summary>
    [property:JsonPropertyName("value")]
    public required TValue Value { get; init; }

    /// <summary>
    /// The document.
    /// </summary>
    [property:JsonPropertyName("doc")]
    public required TDoc Document { get; init; }
}