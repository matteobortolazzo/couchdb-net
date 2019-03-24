using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchDB.Driver.Exceptions
{
    public class CouchNotFoundException : CouchException
    {
        public CouchNotFoundException(string message, string reason) : base(message, reason) { }
    }
}
