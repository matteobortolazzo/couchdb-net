using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;

namespace CouchDB.Driver.Helpers;

internal static class RequestsHelper
{
    public static async Task<T> SendRequestAsync<T>(this Task<T> asyncRequest)
    {
        try
        {
            return await asyncRequest.ConfigureAwait(false);
        }
        catch (CouchHttpResponseException ex)
        {
            CouchError couchError = ex.ResponseContent != null
                ? JsonSerializer.Deserialize<CouchError>(ex.ResponseContent)!
                : new CouchError(null, null);

            throw ex.StatusCode switch
            {
                HttpStatusCode.Conflict => new CouchConflictException(couchError),
                HttpStatusCode.NotFound => new CouchNotFoundException(couchError),
                HttpStatusCode.BadRequest when couchError.Error == "no_usable_index" => new CouchNoIndexException(
                    couchError),
                _ => new CouchException(couchError, ex)
            };
        }
    }
}