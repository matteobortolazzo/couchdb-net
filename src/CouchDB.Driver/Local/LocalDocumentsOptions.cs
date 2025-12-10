using System.ComponentModel;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Local;

public class LocalDocumentsOptions
{
    /// <summary>
    /// Includes conflicts information in response. Ignored if IncludeDocs isn’t <c>True</c>.
    /// </summary>
    [JsonPropertyName("conflicts")]
    [DefaultValue(false)]
    public bool Conflicts { get; init; }

    /// <summary>
    /// Return the change results in descending sequence order (most recent change first).
    /// </summary>
    [JsonPropertyName("descending")]
    [DefaultValue(false)]
    public bool Descending { get; init; }

    /// <summary>
    /// Return the change results in descending sequence order (most recent change first).
    /// </summary>
    [JsonPropertyName("end_key")]
    [DefaultValue(null)]
    public string? EndKey { get; init; }

    /// <summary>
    /// Stop returning records when the specified local document ID is reached.
    /// </summary>
    [JsonPropertyName("end_key_doc_id")]
    [DefaultValue(null)]
    public string? EndKeyDocId { get; init; }
        
    /// <summary>
    /// Specifies whether the specified end key should be included in the result.
    /// </summary>
    [JsonPropertyName("include_docs")]
    [DefaultValue(true)]
    public bool InclusiveEnd { get; init; } = true;

    /// <summary>
    /// Return only local documents that match the specified key.
    /// </summary>
    [JsonPropertyName("key")]
    [DefaultValue(null)]
    public string? Key { get; init; }

    /// <summary>
    /// Limit the number of the returned local documents to the specified number. 
    /// </summary>
    [JsonPropertyName("limit")]
    [DefaultValue(null)]
    public int? Limit { get; init; }

    /// <summary>
    /// Skip this number of records before starting to return the results.
    /// </summary>
    [JsonPropertyName("skip ")]
    [DefaultValue(0)]
    public int Skip { get; init; }

    /// <summary>
    /// Return records starting with the specified key.
    /// </summary>
    [JsonPropertyName("start_key")]
    [DefaultValue(null)]
    public string? StartKey { get; init; }

    /// <summary>
    /// Return records starting with the specified local document ID.
    /// </summary>
    [JsonPropertyName("start_key_doc_id")]
    [DefaultValue(null)]
    public string? StartKeyDocId { get; init; }
}