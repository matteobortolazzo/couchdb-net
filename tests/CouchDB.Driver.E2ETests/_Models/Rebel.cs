using System.Collections.Generic;
using CouchDB.Driver.Attributes;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.E2ETests.Models;

[DatabaseName("rebels")]
public class Rebel : CouchDocument
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public int Age { get; set; }
    public List<string> Skills { get; set; }
}