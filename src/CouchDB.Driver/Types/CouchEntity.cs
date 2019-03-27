using System.Runtime.Serialization;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using Newtonsoft.Json;

namespace CouchDB.Driver.Types
{
    public abstract class CouchEntity
    {
        /// <summary>
        /// The ID of the entity.
        /// </summary>
        [DataMember]
        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        [DataMember]
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        private string IdOther { set => Id = value; }

        /// <summary>
        /// The revision of the current entity.
        /// </summary>
        [DataMember]
        [JsonProperty("_rev", NullValueHandling = NullValueHandling.Ignore)]
        public string Rev { get; set; }
        [DataMember]
        [JsonProperty("rev", NullValueHandling = NullValueHandling.Ignore)]
        private string RevOther { set => Rev = value; }
    }

    internal static class CouchEntityExtensions
    {
        public static CouchEntity ProcessSaveResponse(this CouchEntity item, DocumentSaveResponse response)
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
