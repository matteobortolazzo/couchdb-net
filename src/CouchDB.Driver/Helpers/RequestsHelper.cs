using System;
using System.Net;
using System.Threading.Tasks;
using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Types;
using Flurl.Http;

namespace CouchDB.Driver.Helpers
{
    internal static class RequestsHelper
    {
        public static T SendRequest<T>(this Task<T> asyncRequest)
        {
            return asyncRequest.SendRequestAsync().Result;
        }
        public static async Task<T> SendRequestAsync<T>(this Task<T> asyncRequest)
        {
            try
            {
                return await asyncRequest;
            }
            catch (FlurlHttpException ex)
            {
                var e = await ex.GetResponseJsonAsync<CouchError>();

                if (e == null)
                {
                    throw;
                }

                switch (ex.Call.HttpStatus)
                {
                    case HttpStatusCode.Conflict:
                        throw e.NewCouchExteption(typeof(CouchConflictException));
                    case HttpStatusCode.NotFound:
                        throw e.NewCouchExteption(typeof(CouchNotFoundException));
                    case HttpStatusCode.BadRequest:
                        if (e.Error == "no_usable_index")
                            throw e.NewCouchExteption(typeof(CouchNoIndexException));
                        break;
                }
                throw new CouchException(e.Error, e.Reason);
            }
        }

        private static Exception NewCouchExteption(this CouchError e, Type type)
        {
            var ctor = type.GetConstructor(new[] { typeof(string), typeof(string) });
            var exception = (CouchException)ctor.Invoke(new string[] { e.Error, e.Reason });
            return exception;
        }
    }
}