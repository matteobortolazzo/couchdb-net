using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using CouchDB.Client.Query;
using CouchDB.Client.Query.Extensions;
using Flurl.Http;

namespace CouchDB.Client
{
    public class CouchDatabase<T> where T : CouchEntity
    {
        private readonly CouchClient _client;
        public string Name { get; }

        internal IFlurlRequest NewDbRequest()
        {
            return _client.NewRequest().AppendPathSegment(Name);
        }

        internal CouchDatabase(CouchClient client, string name)
        {
            _client = client;
            Name = name;
            Documents = new CouchDocuments<T>(this);
        }

        public CouchDocuments<T> Documents { get; }

        public async Task NewIndexAsync(Func<ICouchIndexSelector<T>, object> selectorFunc, string name = null, string designDocumentName = null)
        {
            var selector = new CouchIndexSelector<T>();
            selectorFunc.Invoke(selector);
            var fields = selector.IndexFields;

            var indexObject = fields.Select(f => new Dictionary<string, string>{{ f.Name, f.Direction}});
            
            var fieldObject = new ExpandoObject();
            fieldObject.AddProperty("fields", indexObject);

            var requestObject = new ExpandoObject();
            requestObject.AddProperty("index", fieldObject);

            if (name != null)
                requestObject.AddProperty("name", name);
            if(designDocumentName != null)
                requestObject.AddProperty("ddoc", designDocumentName);

            requestObject.AddProperty("type", "json");
            
            await NewDbRequest()
                .AppendPathSegment("_index")
                .PostJsonAsync(requestObject);
        }
    }
}
