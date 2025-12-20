using System.Collections.Generic;

namespace CouchDB.Driver.Example.Models;

public class Rebel
{
    public string Id { get; set; }
    public string Rev { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public int Age { get; set; }
    public List<string> Skills { get; set; }
}