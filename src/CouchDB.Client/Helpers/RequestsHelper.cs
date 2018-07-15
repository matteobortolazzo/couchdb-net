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
                throw new CouchException(e.error, e.reason);
            }
        }
    }
}