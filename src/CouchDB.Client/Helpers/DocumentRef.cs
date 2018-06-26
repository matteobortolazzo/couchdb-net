namespace CouchDB.Client.Helpers
{
    internal class DocumentRef<T>
    {
        public string Id { get; }
        public string Key { get; }
        public string Rev { get; }
        public T Entity { get;  }

        public DocumentRef(string id, string key, string rev, T entity)
        {
            Id = id;
            Key = key;
            Rev = rev;
            Entity = entity;
        }
    }
}
