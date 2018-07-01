using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CouchDB.Client.Helpers;
using CouchDB.Client.Query;
using CouchDB.Client.Query.Selector;
using CouchDB.Client.Responses;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;

namespace CouchDB.Client
{
    public class CouchDatabase<T> where T : CouchEntity
    {
        internal IFlurlRequest BaseRequest { get; }
        public string Name { get; }

        internal CouchDatabase(IFlurlRequest baseUrl, string name)
        {
            Name = name;
            BaseRequest = baseUrl.AppendPathSegment(name);
            Documents = new CouchDocuments<T>(this);
        }

        public CouchDocuments<T> Documents { get; }
    }
}
