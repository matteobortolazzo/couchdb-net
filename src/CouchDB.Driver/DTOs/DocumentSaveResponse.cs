namespace CouchDB.Driver.DTOs
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    internal class DocumentSaveResponse
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
    {
        public bool Ok { get; set; }
        public string Id { get; set; }
        public string Rev { get; set; }
        public string Error { get; set; }
        public string Reason { get; set; }
    }
}
