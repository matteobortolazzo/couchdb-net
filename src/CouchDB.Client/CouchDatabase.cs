using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CouchDB.Client.Helpers;
using CouchDB.Client.Query;
using CouchDB.Client.Query.Extensions;
using CouchDB.Client.Query.Selector;
using CouchDB.Client.Query.Sort;
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

        public async Task NewIndexAsync(Func<ICouchIndexSelector<T>, object> selectorFunc, string name = null, string designDocumentName = null)
        {
            var selector = new CouchIndexSelector<T>();
            selectorFunc.Invoke(selector);
            var fields = selector.IndexFields;

            var indexObject = fields.Select(f => new Dictionary<string, string>{{ f.Name, f.Direction}});

            var requestObject = new ExpandoObject();
            requestObject.AddProperty("index", indexObject);
            
            if(name != null)
                requestObject.AddProperty("name", name);
            if(designDocumentName != null)
                requestObject.AddProperty("ddoc", designDocumentName);

            await BaseRequest
                .AppendPathSegment("_index")
                .PostJsonAsync(requestObject);
        }
    }
}
