using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    /// <summary>
    /// Represents a CouchDB document.
    /// </summary>
    public abstract class CouchDocument
    {
        public CouchDocument()
        {
            Conflicts = new List<string>();
        }

        /// <summary>
        /// The document ID.
        /// </summary>
        [DataMember]
        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string Id { get; set; }
        [DataMember]
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        private string IdOther { set => Id = value; }

        /// <summary>
        /// The current document revision ID.
        /// </summary>
        [DataMember]
        [JsonProperty("_rev", NullValueHandling = NullValueHandling.Ignore)]
        public string Rev { get; set; }
        [DataMember]
        [JsonProperty("rev", NullValueHandling = NullValueHandling.Ignore)]
        private string RevOther { set => Rev = value; }

        [DataMember]
        [JsonProperty("_conflicts")]
        public List<string> Conflicts { get; set; }
    }

    internal static class CouchDocumentExtensions
    {
        public static CouchDocument ProcessSaveResponse(this CouchDocument item, DocumentSaveResponse response)
        {
            if (!response.Ok)
            {
                throw new CouchException(response.Error, response.Reason);
            }

            item.Id = response.Id;
            item.Rev = response.Rev;
            return item;
        }
    }
}
