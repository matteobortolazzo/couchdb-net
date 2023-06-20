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
        /// Includes attachments bodies in response. Default is <c>False</c>
        /// </summary>
        public bool Attachments { get; set; }
        
        /// <summary>
        /// Includes encoding information in attachment stubs if the particular attachment is compressed. Default is <c>False</c>
        /// </summary>
        public bool AttachmentsEncodingInfo { get; set; }
        
        /// <summary>
        /// Includes attachments only since specified revisions. Doesn’t includes attachments for specified revisions. Optional
        /// </summary>
        public IList<string>? AttachmentsSince { get; set; }
        
        /// <summary>
        /// Includes information about conflicts in document. Default is <c>False</c>
        /// </summary>
        public bool Conflicts { get; set; }

        /// <summary>
        /// Includes information about deleted conflicted revisions. Default is <c>False</c>
        /// </summary>
        public bool DeleteConflicts { get; set; }
        
        /// <summary>
        /// Forces retrieving latest “leaf” revision, no matter what rev was requested. Default is <c>False</c>
        /// </summary>
        public bool Latest { get; set; }
        
        /// <summary>
        /// Includes last update sequence for the document. Default is <<c>False</c>
        /// </summary>
        public bool LocalSequence { get; set; }

        /// <summary>
        /// Acts same as specifying all <see cref="Conflicts"/>, <see cref="DeleteConflicts"/> and <see cref="RevisionsInfo"/> query parameters. Default is <c>False</c>
        /// </summary>
        public bool Meta { get; set; }
        
        /// <summary>
        /// Retrieves documents of specified leaf revisions. Additionally, it accepts value as all to return all leaf revisions. Optional
        /// </summary>
        public IList<string>? OpenRevisions { get; set; }
        
        /// <summary>
        /// Retrieves document of specified revision. Optional
        /// </summary>
        public string? Revision { get; set; }
        
        /// <summary>
        /// Includes list of all known document revisions. Default is <c>False</c>
        /// </summary>
        public bool Revisions { get; set; }
        
        /// <summary>
        /// Includes detailed information for all known document revisions. Default is <c>False</c>
        /// </summary>
        public bool RevisionsInfo { get; set; }
    }
}
