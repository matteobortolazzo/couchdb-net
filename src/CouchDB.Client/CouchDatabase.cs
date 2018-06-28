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
        private readonly IFlurlRequest _request;
        private bool _documentsLoaded;
        private readonly List<T> _documents;
        private readonly List<DocumentRef<T>> _documentsRefs;

        public IReadOnlyCollection<T> Documents
        {
            get
            {
                if (!_documentsLoaded)
                    Sync().Wait();
                return _documents;
            }
        }

        public string Name { get; }

        internal CouchDatabase(string name, IFlurlRequest request)
        {
            _request = request;
            Name = name;
            _documents = new List<T>();
            _documentsRefs = new List<DocumentRef<T>>();
        }

        public async Task Sync()
        {
            var docs = await _request
                .AppendPathSegment(Name)
                .AppendPathSegment("_all_docs")
                .GetJsonAsync<DocumentsInfo>();

            foreach (var doc in docs.Rows)
            {
                var request = _request
                    .AppendPathSegment(Name)
                    .AppendPathSegment(doc.Id)
                    .GetJsonAsync<T>();

                var document = await RequestsHelper.SendAsync(request);

                _documents.Add(document);

                var docRef = new DocumentRef<T>(doc.Id, doc.Key, doc.Value.Rev, document);
                _documentsRefs.Add(docRef);
            }

            _documentsLoaded = true;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class FindResult<TResult>
        {
            [JsonProperty("docs")]
            public IEnumerable<TResult> Docs { get; set; }
        }

        public async Task<IEnumerable<TResult>> FindAsync<TResult>(string jsonFilter)
        {
            var request = _request
                .AppendPathSegment(Name)
                .AppendPathSegment("_find")
                .WithHeader("Content-Type", "application/json")
                .PostStringAsync(jsonFilter)
                .ReceiveJson<FindResult<TResult>>();

            var result = await RequestsHelper.SendAsync(request);
            return result.Docs;
        }

        public ICouchQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            var selector = SelectorObjectBuilder.Serialize(predicate);
            var expandoDict = selector as IDictionary<string, object>;
            return new CouchQueryable<T>(expandoDict, _request, Name);
        }

        public T GetDocument(string key)
        {
            var docRef = _documentsRefs.SingleOrDefault(r => r.Key == key);
            return docRef?.Entity;
        }

        public async Task<T> GetDocumentAsync(string key)
        {
            var request = _request
                .AppendPathSegment(Name)
                .AppendPathSegment(key)
                .GetJsonAsync<T>();

            var result = await RequestsHelper.SendAsync(request);
            return result;
        }

        public DocumentInfo GetDocumentInfo(T document)
        {
            var docRef = _documentsRefs.Single(r => r.Entity.Equals(document));
            return new DocumentInfo
            {
                Id = docRef.Id,
                Key = docRef.Key,
                Value = new DocumentInfoValue { Rev = docRef.Rev }
            };
        }

        public async Task AddDocumentAsync(string key, T document)
        {
            var request = _request
                .AppendPathSegment(Name)
                .AppendPathSegment(key)
                .PutJsonAsync(document);

            await RequestsHelper.SendAsync(request);

            _documents.Add(document);
        }

        public async Task AddDocumentsAsync(IDictionary<string, T> documents)
        {
            var docs = documents.Select(document =>
            {
                var docsDict = GetObjectDictionary(document.Value);

                docsDict.Add("_id", document.Key);

                return docsDict;
            });

            var request = _request
                .AppendPathSegment(Name)
                .AppendPathSegment("_buld_docs")
                .PostJsonAsync(new { docs });

            await RequestsHelper.SendAsync(request);
        }

        public async Task UpdateDocumentAsync(T document)
        {
            // Find ref
            var docRef = FindDocumentRef(document);
            // Create the dictionary equivalent of the document
            var docToUpdate = GetObjectDictionary(docRef.Entity);
            // Adds the _rev property
            docToUpdate.Add("_rev", docRef.Rev);

            var request = _request
                .AppendPathSegment(Name)
                .AppendPathSegment(docRef.Id)
                .PutJsonAsync(docToUpdate);

            await RequestsHelper.SendAsync(request);
        }

        public async Task UpdateDocumentsAsync(IEnumerable<T> documents)
        {
            var docs = documents.Select(document =>
            {
                var docRef = FindDocumentRef(document);
                var docsDict = GetObjectDictionary(document);

                docsDict.Add("_id", docRef.Id);
                docsDict.Add("_rev", docRef.Rev);

                return docsDict;
            });

            var request = _request
                .AppendPathSegment(Name)
                .AppendPathSegment("_buld_docs")
                .PostJsonAsync(new { docs });

            await RequestsHelper.SendAsync(request);
        }

        public async Task RemoveDocumentAsync(T document)
        {
            var docRef = _documentsRefs.Single(r => r.Entity.Equals(document));

            var request = _request
                .AppendPathSegment(Name)
                .AppendPathSegment(docRef.Key)
                .SetQueryParam("rev", docRef.Rev)
                .DeleteAsync();

            await RequestsHelper.SendAsync(request);

            _documents.Remove(document);
        }
        
        private DocumentRef<T> FindDocumentRef(T document)
        {
            return _documentsRefs.Single(r => r.Entity.Equals(document));
        }

        private static Dictionary<string, object> GetObjectDictionary(T document)
        {
            var dictionary = document.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(document));
            return dictionary;
        }
    }
}
