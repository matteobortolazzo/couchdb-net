using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text;

namespace CouchDB.Client
{
    internal class MangoQuery
    {
        public HttpMethod Method { get; }
        public string Path { get; }
        public string Body { get; }

        public MangoQuery(HttpMethod method, string path, string body = null)
        {           
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Path = path ?? throw new ArgumentNullException(nameof(body));
            Body = body;
        }
    }
}
