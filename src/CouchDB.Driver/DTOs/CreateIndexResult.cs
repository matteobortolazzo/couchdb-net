using System;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class CreateIndexResult
{
    public required string Result { get; set; }

    public required string Id { get; set; }

    public required string Name { get; set; }
}