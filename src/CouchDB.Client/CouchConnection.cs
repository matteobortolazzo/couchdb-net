using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace CouchDB.Client
{
    public class CouchConnection : IDisposable
    {
        private FlurlClient flurlClient;

        public CouchConnection(string connectionString)
        {
            ConnectionString = connectionString;
            flurlClient = new FlurlClient(connectionString);
        }

        public string ConnectionString { get; private set; }

        internal CouchCommand CreateCommand()
        {
            return new CouchCommand(flurlClient);
        }

        public void Dispose()
        {
            flurlClient.Dispose();
        }
    }
}
