using System;
using CouchDB.Driver.Types;
using CouchDB.Driver.Views;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class CouchViewQueryResult<TKey, TValue, TDoc>
    where TDoc : CouchDocument
{
    /// <summary>
    /// The results in the same order as the queries.
    /// </summary>
    [JsonPropertyName("results")]
    public CouchViewList<TKey, TValue, TDoc>[] Results { get; set; }
}