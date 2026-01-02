namespace CouchDB.Driver.Views;

/// <summary>
/// Base class for a view.
/// </summary>
/// <typeparam name="TKey">The type of the key</typeparam>
/// <typeparam name="TValue">The type of the value</typeparam>
/// <typeparam name="TDoc">The type of the document.</typeparam>
/// <param name="Id">The document ID.</param>
/// <param name="Key">The view key.</param>
/// <param name="Value">The view value.</param>
/// <param name="Document">The document. Returned if included.</param>
[Serializable]
public record CouchView<TKey, TValue, TDoc>(string Id, TKey Key, TValue Value, TDoc? Document);