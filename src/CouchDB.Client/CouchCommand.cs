using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace CouchDB.Client
{
    internal class CouchCommand
    {
        private readonly FlurlClient flurlClient;

        public CouchCommand(FlurlClient flurlClient)
        {
            this.flurlClient = flurlClient;
        }

        public TranslatedRequest Request { get; internal set; }

        public T ExecuteReader<T>() where T : new()
        {
            var request = flurlClient.Request(Request.Path);
            T result;

            if (Request.Method.Equals(HttpMethod.Get))
            {
                result = request.GetJsonAsync<T>().Result;
            }
            else if (Request.Method.Equals(HttpMethod.Post))
            {
                result = request.PostJsonAsync(Request.Body).ReceiveJson<T>().Result;
            }
            else if (Request.Method.Equals(HttpMethod.Put))
            {
                result = request.PutJsonAsync(Request.Body).ReceiveJson<T>().Result;
            }
            else
            {
                throw new NotImplementedException($"HTTP method ${Request.Method.Method} not implemented.");
            }
            return result;
        }
    }
}
