using System.Text.Json;
using System.Threading.Tasks;
using CouchDB.Driver.E2ETests.Models;
using CouchDB.Driver.Options;
using Xunit;

namespace CouchDB.Driver.E2ETests;

public class TestFixture : IAsyncLifetime
{
    public ICouchClient Client = null!;
    public ICouchDatabase<Rebel> Rebels = null!;

    public async Task InitializeAsync()
    {
        var clientOptions = new CouchClientOptions
        {
            JsonSerializerOptions = new JsonSerializerOptions
            {
                TypeInfoResolver = SourceGenerationContext.Default
            }
        };
        Client = new CouchClient(
            endpoint: "http://localhost:5984",
            credentials: new BasicCredentials("admin", "admin"),
            clientOptions);
        Rebels = await Client.GetOrCreateDatabaseAsync<Rebel>();

        await Rebels.CreateIndexAsync("surname_index", builder => builder
            .IndexBy(r => r.Surname)
            .ThenBy(r => r.Name));
    }

    public async Task DisposeAsync()
    {
        await Client.DeleteDatabaseAsync<Rebel>();
        await Client.DisposeAsync();
    }
}