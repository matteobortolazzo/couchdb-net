using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Test.Models
{
    internal class Customer
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }
        public Card Card { get; set; }
        public List<int> Points { get; set; } = new List<int>();
    }
}
