using System;
using System.Collections.Generic;

namespace CouchDB.UnitTests.Models;

public record SimpleRebel(string Name, string Age);

public record Rebel
{
    public string Id { get; set; }
    public string? Rev { get; set; }
    public string Name { get; set; }
    public string? Surname { get; set; }
    public int Age { get; set; }
    public bool IsJedi { get; set; }
    public Species Species { get; set; }
    public Guid Guid { get; set; }
    public List<string> Skills { get; set; }
    public List<Battle> Battles { get; set; }
    public Vehicle? Vehicle { get; set; }
}