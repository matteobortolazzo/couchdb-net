using CouchDB.Driver.Exceptions;
using CouchDB.UnitTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CouchDB.Driver.Query.Extensions;
using CouchDB.Driver.Types;
using CouchDB.Driver.UnitTests._Helpers;
using Xunit;

namespace CouchDB.Driver.UnitTests;

public class Client_Tests : HttpTests
{
    #region Get

    [Fact]
    public async Task GetDatabase_CustomCharacterName()
    {
        var databaseName = "rebel0_$()+/-";

        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        HttpTest.RespondWithJson(new { ok = true });
        var rebels = client.GetDatabase<Rebel>(databaseName);
        Assert.Equal(databaseName, rebels.Database);
    }

    [Fact]
    public async Task GetDatabase_InvalidCharacters_ThrowsArgumentException()
    {
        // Operation result
        HttpTest.RespondWithJson(new { ok = true });
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        Action action = () => client.GetDatabase<Rebel>("rebel.");
        var ex = Assert.Throws<ArgumentException>(action);
        Assert.Contains("invalid characters", ex.Message);
    }

    #endregion

    #region Create

    [Fact]
    public async Task GetOrCreateDatabase_Default()
    {
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        HttpTest.RespondWithJson(new { ok = true });
        var rebels = await client.GetOrCreateDatabaseAsync<Rebel>("rebels");
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithVerb(HttpMethod.Put);
        Assert.Equal("rebels", rebels.Database);
    }

    [Fact]
    public async Task GetOrCreateDatabase_CustomCharacterName()
    {
        var databaseName = "rebel0_$()+/-";

        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        HttpTest.RespondWithJson(new { ok = true });
        var rebels = await client.GetOrCreateDatabaseAsync<Rebel>(databaseName);
        HttpTest
            .ShouldHaveCalled("http://localhost/rebel0_%24%28%29%2B%2F-")
            .WithVerb(HttpMethod.Put);
        Assert.Equal(databaseName, rebels.Database);
    }

    [Fact]
    public async Task GetOrCreateDatabase_402_ReturnDatabase()
    {
        // Operation result
        HttpTest.RespondWith(string.Empty, 412);
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        var rebels = await client.GetOrCreateDatabaseAsync<Rebel>("rebels");

        Assert.NotNull(rebels);

        HttpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithVerb(HttpMethod.Put);
        Assert.Equal("rebels", rebels.Database);
    }

    [Fact]
    public async Task GetOrCreateDatabase_InvalidCharacters_ThrowsArgumentException()
    {
        // Operation result
        HttpTest.RespondWithJson(new { ok = true });
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        Func<Task> action = () => client.GetOrCreateDatabaseAsync<Rebel>("rebel.");
        var ex = await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Contains("invalid characters", ex.Message);
    }

    [Fact]
    public async Task CreateDatabaseAsync_Default()
    {
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        HttpTest.RespondWithJson(new { ok = true });
        var rebels = await client.CreateDatabaseAsync<Rebel>("rebels");
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithVerb(HttpMethod.Put);
        Assert.Equal("rebels", rebels.Database);
    }

    [Fact]
    public async Task CreateDatabase_402_ThrowsException()
    {
        // Operation result
        HttpTest.RespondWith((string)null, 412);
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        Func<Task> action = () => client.CreateDatabaseAsync<Rebel>("rebels");
        await Assert.ThrowsAsync<CouchException>(action);
    }

    [Fact]
    public async Task CreateDatabaseAsync_Params()
    {
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        HttpTest.RespondWithJson(new { ok = true });

        var options = new CreateDatabaseOptions
        {
            Shards = 9,
            Replicas = 2,
            Partitioned = true
        };
        var rebels = await client.CreateDatabaseAsync<Rebel>("rebels", options);
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithQueryParam("q", 9)
            .WithQueryParam("n", 2)
            .WithQueryParam("partitioned", "true")
            .WithVerb(HttpMethod.Put);
        Assert.Equal("rebels", rebels.Database);
    }

    [Fact]
    public async Task CreateDatabaseAsync_Params_Default()
    {
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        HttpTest.RespondWithJson(new { ok = true });

        var options = new CreateDatabaseOptions
        {
            Shards = 8,
            Replicas = 3,
            Partitioned = false
        };
        var rebels = await client.CreateDatabaseAsync<Rebel>("rebels", options);
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithVerb(HttpMethod.Put);
        Assert.Equal("rebels", rebels.Database);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteDatabase_Default()
    {
        // Operation result
        HttpTest.RespondWithJson(new { ok = true });
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        await client.DeleteDatabaseAsync("rebels");
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithVerb(HttpMethod.Delete);
    }

    [Fact]
    public async Task DeleteDatabase_CustomCharacterName()
    {
        // Operation result
        HttpTest.RespondWithJson(new { ok = true });
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        await client.DeleteDatabaseAsync("rebel0_$()+/-");
        HttpTest
            .ShouldHaveCalled("http://localhost/rebel0_%24%28%29%2B%2F-")
            .WithVerb(HttpMethod.Delete);
    }

    [Fact]
    public async Task DeleteDatabase_InvalidCharacters_ThrowsArgumentException()
    {
        // Operation result
        HttpTest.RespondWithJson(new { ok = true });
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        Func<Task> action = () => client.DeleteDatabaseAsync("rebel.");
        var ex = await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Contains("invalid characters", ex.Message);
    }

    #endregion

    #region Utils

    [Fact]
    public async Task Exists()
    {
        // Operation result
        HttpTest.RespondWithJson(new { status = "ok" });
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        var db = "rebel";
        await using var client = TestCouchClientFactory.Create(HttpTest);
        var result = await client.ExistsAsync(db);
        Assert.True(result);

        HttpTest
            .ShouldHaveCalled($"http://localhost/{db}")
            .WithVerb(HttpMethod.Head);
    }

    [Fact]
    public async Task NotExists()
    {
        HttpTest.RespondWith(string.Empty, 404);
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        var db = "rebel";
        await using var client = TestCouchClientFactory.Create(HttpTest);
        var result = await client.ExistsAsync(db);
        Assert.False(result);

        HttpTest
            .ShouldHaveCalled($"http://localhost/{db}")
            .WithVerb(HttpMethod.Head);
    }

    [Fact]
    public async Task IsUp()
    {
        // Operation result
        HttpTest.RespondWithJson(new { status = "ok" });
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        var result = await client.IsUpAsync();
        Assert.True(result);

        HttpTest
            .ShouldHaveCalled($"http://localhost/_up")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task IsNotUp()
    {
        HttpTest.RespondWith(string.Empty, 404);
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        var result = await client.IsUpAsync();
        Assert.False(result);

        HttpTest
            .ShouldHaveCalled($"http://localhost/_up")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task IsNotUp_Timeout()
    {
        HttpTest.SimulateTimeout();
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        var result = await client.IsUpAsync();
        Assert.False(result);

        HttpTest
            .ShouldHaveCalled($"http://localhost/_up")
            .WithVerb(HttpMethod.Get);
    }


    [Fact]
    public async Task DatabaseNames()
    {
        // Databases
        HttpTest.RespondWithJson(new[] { "jedi", "sith" });
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        var dbs = await client.GetDatabasesNamesAsync();
        HttpTest
            .ShouldHaveCalled("http://localhost/_all_dbs")
            .WithVerb(HttpMethod.Get);
        Assert.Equal(["jedi", "sith"], dbs);
    }

    [Fact]
    public async Task ActiveTasks()
    {
        // Tasks
        HttpTest.RespondWithJson(new List<CouchActiveTask>());

        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        var dbs = await client.GetActiveTasksAsync();
        HttpTest
            .ShouldHaveCalled("http://localhost/_active_tasks")
            .WithVerb(HttpMethod.Get);
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task ConflictException()
    {
        HttpTest.RespondWith(string.Empty, (int)HttpStatusCode.Conflict);

        await using var client = TestCouchClientFactory.Create(HttpTest);
        var couchException =
            await Assert.ThrowsAsync<CouchConflictException>(() => client.CreateDatabaseAsync<Rebel>("rebels"));
        Assert.IsType<CouchHttpResponseException>(couchException.InnerException);
    }

    [Fact]
    public async Task NotFoundException()
    {
        HttpTest.RespondWith(string.Empty, (int)HttpStatusCode.NotFound);

        await using var client = TestCouchClientFactory.Create(HttpTest);
        var couchException =
            await Assert.ThrowsAsync<CouchNotFoundException>(() => client.DeleteDatabaseAsync("rebels"));
        Assert.IsType<CouchHttpResponseException>(couchException.InnerException);
    }

    [Fact]
    public async Task BadRequestException()
    {
        HttpTest.RespondWithJson(new
        {
            error = "no_usable_index"
        }, (int)HttpStatusCode.BadRequest);

        await using var client = TestCouchClientFactory.Create(HttpTest);
        var db = client.GetDatabase<Rebel>("rebels");
        var couchException = Assert.Throws<CouchNoIndexException>(() => db.UseIndex("aoeu").ToList());
        Assert.IsType<CouchHttpResponseException>(couchException.InnerException);
    }

    [Fact]
    public async Task GenericExceptionWithMessage()
    {
        const string error = "message text";
        const string reason = "reason text";
        HttpTest.RespondWithJson(new
        {
            error, reason
        }, (int)HttpStatusCode.InternalServerError);

        await using var client = TestCouchClientFactory.Create(HttpTest);
        var db = client.GetDatabase<Rebel>("rebels");
        var couchException = await Assert.ThrowsAsync<CouchException>(() => db.CompactAsync());
        Assert.Equal(error, couchException.Message);
        Assert.Equal(reason, couchException.Reason);
        Assert.IsType<CouchHttpResponseException>(couchException.InnerException);
    }

    [Fact]
    public async Task GenericExceptionNoMessage()
    {
        HttpTest.RespondWith(string.Empty, (int)HttpStatusCode.InternalServerError);

        await using var client = TestCouchClientFactory.Create(HttpTest);
        var db = client.GetDatabase<Rebel>("rebels");
        var couchException = await Assert.ThrowsAsync<CouchException>(() => db.CompactAsync());
        Assert.IsType<CouchHttpResponseException>(couchException.InnerException);
    }

    #endregion
}