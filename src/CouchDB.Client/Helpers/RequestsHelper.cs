using System.Net;
using System.Threading.Tasks;
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
                if (ex.Call.HttpStatus == HttpStatusCode.Conflict)
                {
                    throw new CouchConflictException(e.error, e.reason);
                }
                else
                {
                    throw new CouchException(e.error, e.reason);
                }
            }
        }
    }
}