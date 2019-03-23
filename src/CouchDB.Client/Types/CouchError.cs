using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Client.Types
{
    internal class CouchError
    {
        public string Error { get; set; }
        public string Reason { get; set; }
    }
}
