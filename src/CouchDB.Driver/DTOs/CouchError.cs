namespace CouchDB.Driver.DTOs
{
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    internal class CouchError
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
    {
        public string Error { get; set; }
        public string Reason { get; set; }
    }
}
