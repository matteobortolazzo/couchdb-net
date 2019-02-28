using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    public class CouchConflictException : CouchException
    {
        public CouchConflictException(string message, string reason) : base(message, reason) { }
    }
}
