using System.ComponentModel;
using Newtonsoft.Json;

namespace CouchDB.Driver.ChangesFeed
{
    /// <summary>
    /// Represents the options for the _changes endpoint. 
    /// </summary>
    /// <summary>
    /// Documentation: <see href="https://docs.couchdb.org/en/master/api/database/changes.html">https://docs.couchdb.org/en/master/api/database/changes.html</see>.
    /// </summary>
    public class ChangesFeedOptions
    {
        /// <summary>
        /// Waits until at least one change has occurred, sends the change, then closes the connection.
        /// </summary>
        /// <remarks>
        /// Most commonly used in conjunction with since=now, to wait for the next change. Ignored with continuous feed.
        /// </remarks>
        public bool LongPoll { get; set; }

        /// <summary>
        /// Includes conflicts information in response. Ignored if <see cref="IncludeDocs"/> isn’t <c>True</c>.
        /// </summary>
        [JsonProperty("conflicts")]
        [DefaultValue(false)]
        public bool Conflicts { get; set; }

        /// <summary>
        /// Return the change results in descending sequence order (most recent change first).
        /// </summary>
        [JsonProperty("descending")]
        [DefaultValue(false)]
        public bool Descending { get; set; }

        /// <summary>
        /// Reference to a filter function from a design document that will filter whole stream emitting only filtered events.
        /// </summary>
        [JsonProperty("filter")]
        [DefaultValue(null)]
        public string? Filter { get; set; }

        /// <summary>
        /// Period in milliseconds after which an empty line is sent in the results.
        /// </summary>
        /// <remarks>
        /// Only applicable for <c>longpoll</c>, <c>continuous</c>, and <c>eventsource</c> feeds.
        /// Overrides any timeout to keep the feed alive indefinitely.
        /// </remarks>
        [JsonProperty("heartbeat")]
        [DefaultValue(60000)]
        public int Heartbeat { get; set; } = 60000;

        /// <summary>
        /// Include the associated document with each result. If there are conflicts, only the winning revision is returned. 
        /// </summary>
        [JsonProperty("include_docs")]
        [DefaultValue(false)]
        public bool IncludeDocs { get; set; }

        /// <summary>
        /// Include the Base64-encoded content of attachments in the documents that are included if <see cref="IncludeDocs"/> is <c>True</c>.
        /// </summary>
        [JsonProperty("attachments")]
        [DefaultValue(false)]
        public bool Attachments { get; set; }

        /// <summary>
        /// Include encoding information in attachment stubs if <see cref="IncludeDocs"/> is <c>True</c> and the particular attachment is compressed.
        /// </summary>
        [JsonProperty("att_encoding_info")]
        [DefaultValue(false)]
        public bool AttachEncodingInfo { get; set; }

        /// <summary>
        /// Limit number of result rows to the specified value.
        /// </summary>
        /// <remarks>
        /// Note that using <c>0</c> here has the same effect as <c>1</c>.
        /// </remarks>
        [JsonProperty("limit")]
        [DefaultValue(null)]
        public int? Limit { get; set; }

        /// <summary>
        /// Start the results from the change immediately after the given update sequence. 
        /// </summary>
        /// <remarks>
        /// Can be valid update sequence or <c>now</c> value.
        /// </remarks>
        [JsonProperty("since")]
        [DefaultValue("0")]
        public string? Since { get; set; } = "0";

        /// <summary>
        /// Specifies how many revisions are returned in the changes array. 
        /// </summary>
        /// <remarks>
        /// <see cref="ChangesFeedStyle.MainOnly"/> will only return the current "winning" revision; <see cref="ChangesFeedStyle.AllDocs"/> will return all leaf revisions (including conflicts and deleted former conflicts).
        /// </remarks>
        [JsonProperty("style")]
        [DefaultValue("main_only")]
        public ChangesFeedStyle Style { get; set; } = ChangesFeedStyle.MainOnly;

        /// <summary>
        /// Maximum period in milliseconds to wait for a change before the response is sent, even if there are no results.
        /// </summary>
        /// <remarks>
        /// Only applicable for <c>longpoll</c> or <c>continuous</c> feeds. Default value is specified by <see href="https://docs.couchdb.org/en/master/config/http.html#httpd/changes_timeout">httpd/changes_timeout</see> configuration option.
        /// Note that <c>60000</c> value is also the default maximum timeout to prevent undetected dead connections.
        /// </remarks>
        [JsonProperty("timeout")]
        [DefaultValue(null)]
        public int? Timeout { get; set; }

        /// <summary>
        /// Allows to use view functions as filters. Documents counted as "passed" for view filter in case if map function emits at least one record for them. 
        /// </summary>
        [JsonProperty("view")]
        [DefaultValue(null)]
        public string? View { get; set; }

        /// <summary>
        /// When fetching changes in a batch, setting the seq_interval parameter tells CouchDB to only calculate the update seq with every Nth result returned.
        /// By setting <c>seq_interval=&lt;batch size&gt;</c>, where <c>&lt;batch size&gt;</c> is the number of results requested per batch, load can be reduced on the source CouchDB database;
        /// computing the seq value across many shards (esp. in highly-sharded databases) is expensive in a heavily loaded CouchDB cluster.
        /// </summary>
        [JsonProperty("seq_interval")]
        [DefaultValue(null)]
        public int? SeqInterval { get; set; }
    }
}
