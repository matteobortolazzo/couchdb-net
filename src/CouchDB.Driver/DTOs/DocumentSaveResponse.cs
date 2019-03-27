namespace CouchDB.Driver.DTOs
{
    internal class DocumentSaveResponse
    {
        public bool Ok { get; set; }
        public string Id { get; set; }
        public string Rev { get; set; }
        public string Error { get; set; }
        public string Reason { get; set; }
    }
}
