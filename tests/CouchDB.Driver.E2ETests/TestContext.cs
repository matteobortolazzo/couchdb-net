using CouchDB.Driver.E2E.Models;
using CouchDB.Driver.Settings;

namespace CouchDB.Driver.E2ETests
{
    public class TestContext : CouchDbContext
    {
        public CouchDatabase<Rebel> Rebels { get; set; }

        protected override void OnConfiguring(ICouchContextConfigurator configurator)
        {
            configurator
                .UseEndpoint("http://localhost:5984/")
                .EnsureDatabaseExists()
                .UseBasicAuthentication(username: "admin", password: "admin");
        }
    }
}
