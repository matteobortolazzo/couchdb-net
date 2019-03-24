using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Driver.UnitTests.Models
{
    public class Customer
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public List<string> Hobbies { get; set; }
    }
}
