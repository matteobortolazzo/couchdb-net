using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text;

namespace CouchDB.Client
{
    public class TranslatedRequest
    {
        public string Path { get; set; }
        public HttpMethod Method { get; set; }
        public Dictionary<string, object> Body { get; set; }
    }
}
