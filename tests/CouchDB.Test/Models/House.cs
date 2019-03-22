using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Test.Models
{
    class House
    {
        public Owner Owner { get; set; }
        public List<Floor> Floors { get; set; }
    }
}
