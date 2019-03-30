using CouchDB.Driver.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Driver.E2E.Models
{
    public class Rebel : CouchEntity
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }
        public List<string> Skills { get; set; }
    }
}
