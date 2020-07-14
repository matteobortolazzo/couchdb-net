using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Extensions
{
    internal static class CouchDocumentExtensions
    {
        public static void ProcessSaveResponse(this CouchDocument item, DocumentSaveResponse response)
        {
            if (!response.Ok)
            {
                throw new CouchException(response.Error, null, response.Reason);
            }

            item.Id = response.Id;
            item.Rev = response.Rev;
        }
    }
}
