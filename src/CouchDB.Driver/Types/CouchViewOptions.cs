using System.Collections.Generic;
using System.Linq;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Optional options that can be send with a view request.
    /// </summary>
    public class CouchViewOptions
    {
        /// <summary>
        ///  Include the associated document with each row. Default is false.
        /// </summary>
        public bool? IncludeDocs { get; set; }

        /// <summary>
        /// Include conflicts information in response.
        /// Ignored if include_docs isn’t true. Default is false.
        /// </summary>
        public bool? Conflicts { get; set; }

        /// <summary>
        /// Return records starting with the specified key.
        /// </summary>
        public string? StartKey { get; set; }

        /// <summary>
        /// Stop returning records when the specified key is reached.
        /// </summary>
        public string? EndKey { get; set; }

        /// <summary>
        ///  Specifies whether the specified end key should
        ///  be included in the result. Default is true.
        /// </summary>
        public bool? InclusiveEnd { get; set; }

        /// <summary>
        ///  Return the documents in descending order by key. Default is false.
        /// </summary>
        public bool? Descending { get; set; }

        /// <summary>
        ///  Group the results using the reduce function to a group or single row.
        ///  Implies reduce is true and the maximum group_level. Default is false.
        /// </summary>
        public bool? Group { get; set; }

        /// <summary>
        /// Include encoding information in attachment stubs if include_docs is true
        /// and the particular attachment is compressed.
        /// Ignored if include_docs isn’t true. Default is false.
        /// </summary>
        public bool? AttEncodingInfo { get; set; }

        /// <summary>
        /// Use the reduction function. Default is true
        /// when a reduce function is defined.
        /// </summary>
        public bool? Reduce { get; set; }

        /// <summary>
        /// Whether or not the view results should be returned
        /// from a stable set of shards. Default is false.
        /// </summary>
        public bool? Stable { get; set; }

        /// <summary>
        /// Whether to include in the response an update_seq value indicating the
        /// sequence id of the database the view reflects. Default is false.
        /// </summary>
        public bool? UpdateSeq { get; set; }

        /// <summary>
        /// Include the Base64-encoded content of
        /// <see href="https://docs.couchdb.org/en/stable/api/document/common.html#api-doc-attachments">attachments</see>
        /// in the documents that are included if include_docs is true.
        /// Ignored if include_docs isn’t true. Default is false.
        /// </summary>
        public bool? Attachments { get; set; }

        /// <summary>
        /// Sort returned rows. Setting this to false offers a performance boost.
        /// The total_rows and offset fields are not available when this is set to false.
        /// Default is true.
        /// </summary>
        public bool? Sorted { get; set; }

        /// <summary>
        /// Specify the group level to be used. Implies group is true.
        /// </summary>
        public int? GroupLevel { get; set; }

        /// <summary>
        /// Limit the number of the returned documents to the specified number.
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        ///  Skip this number of records before starting to return the results.
        ///  Default is 0.
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Whether or not the view in question should be updated prior to responding to the user.
        /// Supported values: true, false, lazy. Default is true.
        /// </summary>
        public string? Update { get; set; }

        /// <summary>
        ///  Stop returning records when the specified document ID is reached.
        ///  Ignored if endkey is not set.
        /// </summary>
        public string? EndkeyDocId { get; set; }

        /// <summary>
        /// Return only documents that match the specified key.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Return records starting with the specified document ID.
        /// Ignored if startkey is not set.
        /// </summary>
        public string? StartkeyDocid { get; set; }

        // Error CA2227: Change 'Keys' to be read-only by removing the property setter.
        // Disable this so keys can be added in with other properties on initialization.
#pragma warning disable CA2227
        /// <summary>
        /// Return only documents where the key matches one of the keys specified in the array.
        /// </summary>
        public HashSet<string>? Keys { get; set; }

#pragma warning restore CA2227

        public object ToQueryParameters() => new
        {
            include_docs = IncludeDocs,
            conflicts = Conflicts,
            startkey = TryEscape(StartKey),
            endKey = TryEscape(EndKey),
            inclusive_end = InclusiveEnd,
            descending = Descending,
            group = Group,
            att_encoding_info = AttEncodingInfo,
            reduce = Reduce,
            stable = Stable,
            update_seq = UpdateSeq,
            attachments = Attachments,
            sorted = Sorted,
            group_level = GroupLevel,
            limit = Limit,
            skip = Skip,
            update = TryEscape(Update),
            endkey_docid = TryEscape(EndkeyDocId),
            key = TryEscape(Key),
            startkey_docid = TryEscape(StartkeyDocid),
            keys = Keys is null ? null : $"[{string.Join(',', Keys.Select(Escape))}]"
        };

        private static string Escape(string str) =>
            $"\"{str}\"";

        private static string? TryEscape(string? str) =>
            str is null ? null : Escape(str);
    }
}