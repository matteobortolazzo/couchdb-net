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
        Client = new CouchClient(
            endpoint: "http://localhost:5984",
            credentials: new BasicCredentials("admin", "admin"));
        Rebels = await Client.GetOrCreateDatabaseAsync<Rebel>();
    }

    public async Task DisposeAsync()
    {
        await Client.DeleteDatabaseAsync<Rebel>();
        await Client.DisposeAsync();
    }
}