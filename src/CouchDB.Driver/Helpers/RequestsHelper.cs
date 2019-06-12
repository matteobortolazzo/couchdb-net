using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using Flurl.Http;
using Nito.AsyncEx;

namespace CouchDB.Driver.Helpers
{
    internal static class RequestsHelper
    {
        public static T SendRequest<T>(this Task<T> asyncRequest)
        {
            return AsyncContext.Run(() => asyncRequest.SendRequestAsync());
        }
        public static async Task<T> SendRequestAsync<T>(this Task<T> asyncRequest)
        {
            try
            {
                return await asyncRequest.ConfigureAwait(false);
            }
            catch (FlurlHttpException ex)
            {
                CouchError couchError = await ex.GetResponseJsonAsync<CouchError>().ConfigureAwait(false);

                if (couchError == null)
                {
                    couchError = new CouchError();
                }

                switch (ex.Call.HttpStatus)
                {
                    case HttpStatusCode.Conflict:
                        throw new CouchConflictException(couchError, ex);
                    case HttpStatusCode.NotFound:
                        throw new CouchNotFoundException(couchError, ex);
                    case HttpStatusCode.BadRequest when couchError.Error == "no_usable_index":
                        throw new CouchNoIndexException(couchError, ex);
                    default:
                        throw new CouchException(couchError, ex);
                }
            }
        }
    }
}