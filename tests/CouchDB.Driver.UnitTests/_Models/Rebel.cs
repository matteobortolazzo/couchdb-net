using CouchDB.Driver.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Driver.UnitTests.Models
{
    public class Rebel : CouchDocument
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }
        public Species Species { get; set; }
        public List<string> Skills { get; set; }
        public List<Battle> Battles { get; set; }
    }
}
