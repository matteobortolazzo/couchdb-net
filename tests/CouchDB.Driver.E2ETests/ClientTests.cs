using System.Threading.Tasks;
using CouchDB.Driver.E2ETests.Models;
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

        var response = await users.AddAsync(new CouchUser("luke", "lasersword"));

        var findResponse = await users.FindAsync(response.Id);
        Assert.NotNull(findResponse);

        var luke = findResponse.Document;
        Assert.Equal("luke", luke.Name);

        luke = luke with { Password = "r2d2" };
        response = await users.ReplaceAsync(luke, luke.Id, findResponse.Rev);

        await users.DeleteAsync(luke.Id, response.Rev);
        findResponse = await users.FindAsync(luke.Id);
        Assert.Null(findResponse);

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