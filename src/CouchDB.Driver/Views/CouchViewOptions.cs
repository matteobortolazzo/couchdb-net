using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Views;

/// <summary>
/// Optional parameters to use when getting a view.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
[Serializable]
public class CouchViewOptions<TKey>
{
    /// <summary>
    /// Include conflicts information in response.
    /// Ignored if <see cref="IncludeDocs"/> isn't <c>True</c>. Default is <c>False</c>.
    /// </summary>
    [JsonPropertyName("conflicts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Conflicts { get; init; }

    /// <summary>
    /// Return the documents in descending order by key. Default is <c>False</c>.
    /// </summary>
    [JsonPropertyName("descending")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Descending { get; init; }

    /// <summary>
    /// Stop returning records when the specified key is reached.
    /// </summary>
    [JsonPropertyName("endkey")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TKey? EndKey { get; init; }

    /// <summary>
    ///  Stop returning records when the specified document ID is reached.
    ///  Ignored if <see cref="EndKey"/> is not set.
    /// </summary>
    [JsonPropertyName("endkey_docid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EndKeyDocId { get; init; }

    /// <summary>
    ///  Group the results using the reduce function to a group or single row.
    ///  Implies reduce is <c>True</c> and the maximum <see cref="GroupLevel"/>. Default is <c>False</c>.
    /// </summary>
    [JsonPropertyName("group")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Group { get; init; }

    /// <summary>
    /// Specify the group level to be used. Implies group is <c>True</c>.
    /// </summary>
    [JsonPropertyName("group_level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? GroupLevel { get; init; }

    /// <summary>
    ///  Include the associated document with each row. Default is <c>False</c>.
    /// </summary>
    [JsonPropertyName("include_docs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IncludeDocs { get; init; }

    /// <summary>
    /// Include the Base64-encoded content of attachments in the documents that are included if <see cref="IncludeDocs"/> is <c>True</c>.
    /// Ignored if <see cref="IncludeDocs"/> isn’t <c>True</c>. Default is <c>False</c>.
    /// </summary>
    [JsonPropertyName("attachments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Attachments { get; init; }

    /// <summary>
    /// Include encoding information in attachment stubs if <see cref="IncludeDocs"/> is <c>True</c> and the particular attachment is compressed.
    /// Ignored if <see cref="IncludeDocs"/> isn’t <c>True</c>. Default is <c>False</c>.
    /// </summary>
    [JsonPropertyName("att_encoding_info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AttachEncodingInfo { get; init; }

    /// <summary>
    ///  Specifies whether the specified end key should be included in the result. Default is <c>True</c>.
    /// </summary>
    [JsonPropertyName("inclusive_end")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? InclusiveEnd { get; init; }

    /// <summary>
    /// Return only documents that match the specified key.
    /// </summary>
    [JsonPropertyName("key")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TKey? Key { get; init; }

    /// <summary>
    /// Return only documents where the key matches one of the keys specified in the array.
    /// </summary>
    [JsonPropertyName("keys")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<TKey>? Keys { get; init; }

    /// <summary>
    /// Limit the number of the returned documents to the specified number.
    /// </summary>
    [JsonPropertyName("limit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Limit { get; init; }

    /// <summary>
    /// Use the reduction function. Default is <c>True</c> when a reduce function is defined.
    /// </summary>
    [JsonPropertyName("reduce")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Reduce { get; init; }

    /// <summary>
    /// Skip this number of records before starting to return the results. Default is <code>0</code>.
    /// </summary>
    [JsonPropertyName("skip")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Skip { get; init; }

    /// <summary>
    /// Sort returned rows (see Sorting <see href="https://docs.couchdb.org/en/stable/api/ddoc/views.html#api-ddoc-view-sorting"></see> Returned Rows).
    /// Setting this to false offers a performance boost.
    /// The <see cref="CouchViewResult{TKey, TRow}.TotalRows"/> and <see cref="CouchViewResult{TKey, TRow}.Offset"/> fields are not available when this is set to <c>False</c>.
    /// Default is <c>True</c>.
    /// </summary>
    [JsonPropertyName("sorted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Sorted { get; init; }

    /// <summary>
    /// Whether or not the view results should be returned from a stable set of shards.
    /// Supported values <see cref="StableStyle.True"/>, <see cref="StableStyle.False"/>. Default is <see cref="StableStyle.False"/>
    /// </summary>
    [JsonIgnore]
    public StableStyle? Stable { get; init; }

    [JsonPropertyName("stable")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    internal string? StableString => Stable?.ToString();

    /// <summary>
    /// Return records starting with the specified key.
    /// </summary>
    [JsonPropertyName("startkey")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TKey? StartKey { get; init; }

    /// <summary>
    /// Return records starting with the specified document ID. Ignored if <see cref="StartKey"/> is not set.
    /// </summary>
    [JsonPropertyName("startkey_docid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StartKeyDocId { get; init; }

    /// <summary>
    /// Whether or not the view in question should be updated prior to responding to the user.
    /// Supported values: <see cref="UpdateStyle.True"/>, <see cref="UpdateStyle.False"/>, <see cref="UpdateStyle.Lazy"/>. Default is <see cref="UpdateStyle.True"/>.
    /// </summary>
    [JsonIgnore]
    public UpdateStyle? Update { get; init; }

    [JsonPropertyName("update")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    internal string? UpdateString => Update?.ToString();

    /// <summary>
    /// Whether to include in the response an <see cref="UpdateSeq"/> value indicating the sequence id of the database the view reflects.
    /// Default is <c>False</c>.
    /// </summary>
    [JsonPropertyName("update_seq")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? UpdateSeq { get; init; }
}