using CouchDB.Driver.Types;
using CouchDB.Driver.Views;
using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace CouchDB.Driver.DTOs
{
    internal class CouchViewQueryResult<TKey, TValue, TDoc>
        where TDoc : CouchDocument
    {
        /// <summary>
        /// The results in the same order as the queries.
        /// </summary>
        [JsonProperty("results")]
        public CouchViewList<TKey, TValue, TDoc>[] Results { get; set; }
    }
}
#nullable restore