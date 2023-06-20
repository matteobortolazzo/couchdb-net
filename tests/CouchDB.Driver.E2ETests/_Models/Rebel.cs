using System.Collections.Generic;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.E2ETests.Models
{
    public class Rebel : CouchDocument
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }
        public List<string> Skills { get; set; }
    }
}
