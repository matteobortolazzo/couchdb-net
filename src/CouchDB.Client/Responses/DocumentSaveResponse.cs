using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchDB.Client.Responses
{
    public class DocumentSaveResponse
    {
        public bool Ok { get; set; }

        public string Id { get; set; }

        public string Rev { get; set; }

        public string Error { get; set; }

        public string Reason { get; set; }
    }
}
