using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Driver.DatabaseApiMethodOptions
{
    /// <summary>
    /// Options relevant to saving a new document (supported by both PUT /{db}/{docid} and POST /{db}).
    /// Check https://docs.couchdb.org/en/stable/api/database/common.html#post--db
    /// Check https://docs.couchdb.org/en/stable/api/document/common.html#put--db-docid
    /// </summary>
    public class AddOptions
    {
        /// <summary>
        /// Stores document in batch mode. Check https://docs.couchdb.org/en/stable/api/database/common.html#api-doc-batch-writes
        /// </summary>
        public bool Batch { get; set; } = false;
    }
}
