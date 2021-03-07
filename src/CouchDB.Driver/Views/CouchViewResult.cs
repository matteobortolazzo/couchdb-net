#nullable disable
#pragma warning disable CA2227 // Collection properties should be read only
using System.Collections.Generic;
using CouchDB.Driver.Types;
using Newtonsoft.Json;

namespace CouchDB.Driver.Views
{
    /// <summary>
    /// Result of a view query.
    /// </summary>
    /// <typeparam name="TKey">The type of the value that will be returned.</typeparam>
    /// <typeparam name="TValue">The type of the value that will be returned.</typeparam>
    public class CouchViewResult<TKey, TValue>
    {
        /// <summary>
        /// Number of documents in the database/view.
        /// </summary>
        [JsonProperty("total_rows")]
        public int TotalRows { get; set; }

        /// <summary>
        /// Offset where the document list started.
        /// </summary>
        [JsonProperty("offset")]
        public int Offset { get; set; }

        /// <summary>
        /// Array of view row objects. This result contains only the document ID and revision.
        /// </summary>
        [JsonProperty("rows")]
        public IList<CouchViewRow<TKey, TValue>> Rows { get; set; }
    }
}
#pragma warning restore CA2227 // Collection properties should be read only
#nullable restore