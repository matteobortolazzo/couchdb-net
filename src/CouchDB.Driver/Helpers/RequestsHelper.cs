using System.Net;
using System.Threading.Tasks;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using Flurl.Http;

namespace CouchDB.Driver.Helpers
{
    internal static class RequestsHelper
    {
        public static async Task<TResult> SendRequestAsync<TResult>(this Task<TResult> asyncRequest)
        {
            try
            {
                return await asyncRequest.ConfigureAwait(false);
            }
            catch (FlurlHttpException ex)
            {
                CouchError couchError = await ex.GetResponseJsonAsync<CouchError>().ConfigureAwait(false) ?? new CouchError();

                throw (HttpStatusCode?)ex.StatusCode switch
                {
                    HttpStatusCode.Conflict => new CouchConflictException(couchError, ex),
                    HttpStatusCode.NotFound => new CouchNotFoundException(couchError, ex),
                    HttpStatusCode.BadRequest when couchError.Error == "no_usable_index" => new CouchNoIndexException(
                        couchError, ex),
                    _ => new CouchException(couchError, ex)
                };
            }
        }
    }
}