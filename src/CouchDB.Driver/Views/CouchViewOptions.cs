#pragma warning disable CA2227 // Collection properties should be read only
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CouchDB.Driver.Views
{
    /// <summary>
    /// Optional parameters to use when getting a view.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public class CouchViewOptions<TKey>
    {
        /// <summary>
        /// Include conflicts information in response.
        /// Ignored if <see cref="IncludeDocs"/> isn't <c>True</c>. Default is <c>False</c>.
        /// </summary>
        [JsonProperty("conflicts", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Conflicts { get; set; }

        /// <summary>
        /// Return the documents in descending order by key. Default is <c>False</c>.
        /// </summary>
        [JsonProperty("descending", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Descending { get; set; }

        /// <summary>
        /// Stop returning records when the specified key is reached.
        /// </summary>
        [JsonProperty("endkey", NullValueHandling = NullValueHandling.Ignore)]
        public TKey? EndKey { get; set; }

        /// <summary>
        ///  Stop returning records when the specified document ID is reached.
        ///  Ignored if <see cref="EndKey"/> is not set.
        /// </summary>
        [JsonProperty("endkey_docid", NullValueHandling = NullValueHandling.Ignore)]
        public string? EndKeyDocId { get; set; }

        /// <summary>
        ///  Group the results using the reduce function to a group or single row.
        ///  Implies reduce is <c>True</c> and the maximum <see cref="GroupLevel"/>. Default is <c>False</c>.
        /// </summary>
        [JsonProperty("group", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Group { get; set; }

        /// <summary>
        /// Specify the group level to be used. Implies group is <c>True</c>.
        /// </summary>
        [JsonProperty("group_level", NullValueHandling = NullValueHandling.Ignore)]
        public int? GroupLevel { get; set; }

        /// <summary>
        ///  Include the associated document with each row. Default is <c>False</c>.
        /// </summary>
        [JsonProperty("include_docs", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IncludeDocs { get; set; }

        /// <summary>
        /// Include the Base64-encoded content of attachments in the documents that are included if <see cref="IncludeDocs"/> is <c>True</c>.
        /// Ignored if <see cref="IncludeDocs"/> isn’t <c>True</c>. Default is <c>False</c>.
        /// </summary>
        [JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Attachments { get; set; }

        /// <summary>
        /// Include encoding information in attachment stubs if <see cref="IncludeDocs"/> is <c>True</c> and the particular attachment is compressed.
        /// Ignored if <see cref="IncludeDocs"/> isn’t <c>True</c>. Default is <c>False</c>.
        /// </summary>
        [JsonProperty("att_encoding_info", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AttachEncodingInfo { get; set; }

        /// <summary>
        ///  Specifies whether the specified end key should be included in the result. Default is <c>True</c>.
        /// </summary>
        [JsonProperty("inclusive_end", NullValueHandling = NullValueHandling.Ignore)]
        public bool? InclusiveEnd { get; set; }

        /// <summary>
        /// Return only documents that match the specified key.
        /// </summary>
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public TKey? Key { get; set; }

        /// <summary>
        /// Return only documents where the key matches one of the keys specified in the array.
        /// </summary>
        [JsonProperty("keys", NullValueHandling = NullValueHandling.Ignore)]
        public IList<TKey>? Keys { get; set; }

        /// <summary>
        /// Limit the number of the returned documents to the specified number.
        /// </summary>
        [JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)]
        public int? Limit { get; set; }

        /// <summary>
        /// Use the reduction function. Default is <c>True</c> when a reduce function is defined.
        /// </summary>
        [JsonProperty("reduce", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Reduce { get; set; }

        /// <summary>
        /// Skip this number of records before starting to return the results. Default is <code>0</code>.
        /// </summary>
        [JsonProperty("skip", NullValueHandling = NullValueHandling.Ignore)]
        public int? Skip { get; set; }

        /// <summary>
        /// Sort returned rows (see Sorting <see href="https://docs.couchdb.org/en/stable/api/ddoc/views.html#api-ddoc-view-sorting"></see> Returned Rows).
        /// Setting this to false offers a performance boost.
        /// The <see cref="CouchViewResult{TKey, TRow}.TotalRows"/> and <see cref="CouchViewResult{TKey, TRow}.Offset"/> fields are not available when this is set to <c>False</c>.
        /// Default is <c>True</c>.
        /// </summary>
        [JsonProperty("sorted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Sorted { get; set; }

        /// <summary>
        /// Whether or not the view results should be returned from a stable set of shards.
        /// Supported values <see cref="StableStyle.True"/>, <see cref="StableStyle.False"/>. Default is <see cref="StableStyle.False"/>
        /// </summary>
        [JsonIgnore]
        public StableStyle? Stable { get; set; }

        [JsonProperty("stable", NullValueHandling = NullValueHandling.Ignore)]
        internal string? StableString => Stable?.ToString();

        /// <summary>
        /// Return records starting with the specified key.
        /// </summary>
        [JsonProperty("startkey", NullValueHandling = NullValueHandling.Ignore)]
        public TKey? StartKey { get; set; }

        /// <summary>
        /// Return records starting with the specified document ID. Ignored if <see cref="StartKey"/> is not set.
        /// </summary>
        [JsonProperty("startkey_docid", NullValueHandling = NullValueHandling.Ignore)]
        public string? StartKeyDocId { get; set; }

        /// <summary>
        /// Whether or not the view in question should be updated prior to responding to the user.
        /// Supported values: <see cref="UpdateStyle.True"/>, <see cref="UpdateStyle.False"/>, <see cref="UpdateStyle.Lazy"/>. Default is <see cref="UpdateStyle.True"/>.
        /// </summary>
        [JsonIgnore]
        public UpdateStyle? Update { get; set; }

        [JsonProperty("update", NullValueHandling = NullValueHandling.Ignore)]
        internal string? UpdateString => Update?.ToString();

        /// <summary>
        /// Whether to include in the response an <see cref="UpdateSeq"/> value indicating the sequence id of the database the view reflects.
        /// Default is <c>False</c>.
        /// </summary>
        [JsonProperty("update_seq", NullValueHandling = NullValueHandling.Ignore)]
        public bool? UpdateSeq { get; set; }
    }
}
#pragma warning restore CA2227 // Collection properties should be read only
