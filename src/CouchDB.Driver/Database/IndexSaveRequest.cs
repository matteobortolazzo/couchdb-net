namespace CouchDB.Driver.Database
{
    internal class IndexSaveRequest
    {
        public string? Type { get; set; }
        public string? Name { get; set; }
        public IndexSaveRequestFields? Index { get; set; }
    }
}