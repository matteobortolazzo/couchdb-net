using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CouchDB.Client.Query.Selector;
using CouchDB.Client.Query.Selector.Nodes;
using Flurl.Http;
using Newtonsoft.Json;

namespace CouchDB.Client.Query
{
    public class CouchQueryable<T> 
    {
        private readonly IDictionary<string, object> _selector;
        private readonly IFlurlRequest _baseRequest;
        private readonly string _dbName;

        internal CouchQueryable(IDictionary<string, object> selector, IFlurlRequest baseRequest, string dbName)
        {
            _selector = selector;
            _baseRequest = baseRequest;
            _dbName = dbName;
        }

        public class FindResult
        {
            [JsonProperty("docs")]
            public IEnumerable<T> Docs { get; set; }
        }

        public async Task<IEnumerable<T>> ToListAsync()
        {
            var jsonOb = new
            {
                selector = _selector
            };

            var j = JsonConvert.SerializeObject(jsonOb);
            
            var result = await _baseRequest
                .AppendPathSegment(_dbName)
                .AppendPathSegment("_find")
                .PostJsonAsync(new
                {
                    selector = _selector
                })
                .ReceiveJson<FindResult>();

            return result.Docs;
        }
    }
}
