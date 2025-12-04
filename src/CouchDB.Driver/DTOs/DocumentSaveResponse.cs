using System;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class DocumentSaveResponse
{
    public bool Ok { get; set; }
    public required string Id { get; set; }
    public required string Rev { get; set; }
    public required string Error { get; set; }
    public required string Reason { get; set; }
}