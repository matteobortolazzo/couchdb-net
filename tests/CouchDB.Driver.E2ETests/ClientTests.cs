using System.Threading.Tasks;
using CouchDB.Driver.E2ETests.Models;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Types;
using Xunit;

namespace CouchDB.Driver.E2ETests;

[Trait("Category", "Integration")]
public class ClientTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task Users()
    {
        var users = await fixture.Client.GetOrCreateUsersDatabaseAsync();

        var luke = await users.AddAsync(new CouchUser(name: "luke", password: "lasersword"));
        Assert.Equal("luke", luke.Name);

        luke = await users.FindAsync(luke.Id);
        Assert.Equal("luke", luke.Name);

        luke = await users.ChangeUserPassword(luke, "r2d2");

        await users.RemoveAsync(luke);
        luke = await users.FindAsync(luke.Id);
        Assert.Null(luke);

        await fixture.Client.DeleteDatabaseAsync<CouchUser>();
    }

    [Fact]
    public async Task Exists()
    {
        var exists = await fixture.Client.ExistsAsync<Rebel>();

        Assert.True(exists);
    }

    [Fact]
    public async Task IsUp()
    {
        var isUp = await fixture.Client.IsUpAsync();

        Assert.True(isUp);
    }

    [Fact]
    public async Task GetDatabasesNames()
    {
        var dbs = await fixture.Client.GetDatabasesNamesAsync();

        Assert.NotEmpty(dbs);
    }
}