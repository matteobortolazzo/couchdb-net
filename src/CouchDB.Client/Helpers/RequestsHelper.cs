using System.Net;
using System.Threading.Tasks;
using CouchDB.Client.Exceptions;
using Flurl.Http;

namespace CouchDB.Client.Helpers
{
    internal static class RequestsHelper
    {
        internal static async Task<T> SendAsync<T>(this Task<T> asyncRequest)
        {
            try
            {
                return await asyncRequest;
            }
            catch (FlurlHttpException ex)
            {
                var e = await ex.GetResponseJsonAsync();

                if (e == null)
                {
                    throw;
                }

                switch (ex.Call.HttpStatus)
                {
                    case HttpStatusCode.Conflict:
                        throw new CouchConflictException(e.error, e.reason);
                    case HttpStatusCode.NotFound:
                        throw new CouchNotFoundException(e.error, e.reason);
                    default:
                        throw new CouchException(e.error, e.reason);
                }
            }
        }
    }
}