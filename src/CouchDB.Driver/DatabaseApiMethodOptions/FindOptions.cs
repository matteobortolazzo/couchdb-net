using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Driver.DatabaseApiMethodOptions
{
    /// <summary>
    /// Options relevant to getting a document (supported by GET HTTP-method).
    /// Check https://docs.couchdb.org/en/stable/api/document/common.html#get--db-docid
    /// </summary>
    public class FindOptions
    {
        /// <summary>
        /// Includes information about conflicts in document. Default is false
        /// </summary>
        public bool Conflicts { get; set; } = false;

        /// <summary>
        /// Retrieves document of specified revision. Optional
        /// </summary>
        public string? Rev { get; set; }
    }
}
