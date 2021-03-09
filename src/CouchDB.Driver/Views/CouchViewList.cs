using System.Collections.Generic;
using CouchDB.Driver.Types;
using Newtonsoft.Json;

#nullable disable
#pragma warning disable CA2227 // Collection properties should be read only
namespace CouchDB.Driver.Views
{
    /// <summary>
    /// Result of a view query.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TDoc">The type of the document.</typeparam>
    public class CouchViewList<TKey, TValue, TDoc>
        where TDoc : CouchDocument
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
        /// Array of view row objects. This result contains the document ID, value and the documents.
        /// </summary>
        [JsonProperty("rows")]
        public List<CouchView<TKey, TValue, TDoc>> Rows { get; set; }
    }
}
#pragma warning restore CA2227 // Collection properties should be read only
#nullable restore