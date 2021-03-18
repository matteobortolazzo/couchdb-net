namespace CouchDB.Driver.Options
{
    public abstract class CouchDocumentBuilder
    {
        internal string? Database { get; set; }
        internal int? Shards { get; set; }
        internal int? Replicas { get; set; }
        internal bool Partitioned { get; set; }
        internal string? Discriminator { get; set; }
    }    
}