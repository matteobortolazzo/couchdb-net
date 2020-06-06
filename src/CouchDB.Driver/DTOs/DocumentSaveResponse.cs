#nullable disable
namespace CouchDB.Driver.DTOs
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
    internal class DocumentSaveResponse
    {
        public bool Ok { get; set; }
        public string Id { get; set; }
        public string Rev { get; set; }
        public string Error { get; set; }
        public string Reason { get; set; }
    }
}
#nullable restore