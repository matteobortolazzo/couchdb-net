using System.Threading.Tasks;
using CouchDB.Driver.E2ETests.Models;
using Xunit;

namespace CouchDB.Driver.E2ETests;

public class TestFixture : IAsyncLifetime
{
    public ICouchClient Client = null!;
    public ICouchDatabase<Rebel> Rebels = null!;

    public async Task InitializeAsync()
    {
        Client = new CouchClient("http://localhost:5984", c =>
            c.UseBasicAuthentication("admin", "admin"));
        // ensure the _users database exists to prevent couchdb from
        // generating tons of errors in the logs
        await Client.GetOrCreateUsersDatabaseAsync();
        Rebels = await Client.GetOrCreateDatabaseAsync<Rebel>();
    }

    public async Task DisposeAsync()
    {
        await Client.DeleteDatabaseAsync<Rebel>();
        await Client.DisposeAsync();
    }
}