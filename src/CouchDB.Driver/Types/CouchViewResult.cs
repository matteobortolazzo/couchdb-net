using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Object that stores results of a view.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class CouchViewResult<TValue>
    {
        /// <summary>
        /// Number of documents in the database/view.
        /// </summary>
        [JsonProperty("total_rows")]
        public int TotalRows { get; private set; }

        /// <summary>
        /// Offset where the document list started.
        /// </summary>
        [JsonProperty("offset")]
        public int Offset { get; private set; }

        /// <summary>
        /// Array of view row objects. This result contains only the document ID and revision.
        /// </summary>
        [JsonProperty("rows")]
        public IReadOnlyList<CouchViewRow<TValue>> Rows { get; private set; } = ImmutableArray.Create<CouchViewRow<TValue>>();
    }

    /// <summary>
    /// Object that stores results of a view.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TDoc">The type of the doc.</typeparam>
    public class CouchViewResult<TValue, TDoc>
        where TDoc : CouchDocument

    {
        /// <summary>
        /// Number of documents in the database/view.
        /// </summary>
        [JsonProperty("total_rows")]
        public int TotalRows { get; private set; }

        /// <summary>
        /// Offset where the document list started.
        /// </summary>
        [JsonProperty("offset")]
        public int Offset { get; private set; }

        /// <summary>
        /// Array of view row objects. This result contains the document ID, revision and the documents.
        /// </summary>
        [JsonProperty("rows")]
        public IReadOnlyList<CouchViewRow<TValue, TDoc>> Rows { get; private set; } = ImmutableArray.Create<CouchViewRow<TValue, TDoc>>();
    }
}