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

public class Client_Tests : HttpTest
{
    #region Get

    [Fact]
    public async Task GetDatabase_CustomCharacterName()
    {
        var databaseName = "rebel0_$()+/-";

        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        httpTest.RespondWithJson(new { ok = true });
        var rebels = client.GetDatabase<Rebel>(databaseName);
        Assert.Equal(databaseName, rebels.Database);
    }

    [Fact]
    public async Task GetDatabase_InvalidCharacters_ThrowsArgumentException()
    {
        // Operation result
        httpTest.RespondWithJson(new { ok = true });
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
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
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        httpTest.RespondWithJson(new { ok = true });
        var rebels = await client.GetOrCreateDatabaseAsync<Rebel>();
        httpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithVerb(HttpMethod.Put);
        Assert.Equal("rebels", rebels.Database);
    }

    [Fact]
    public async Task GetOrCreateDatabase_CustomName()
    {
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        httpTest.RespondWithJson(new { ok = true });
        var rebels = await client.GetOrCreateDatabaseAsync<Rebel>("some_rebels");
        httpTest
            .ShouldHaveCalled("http://localhost/some_rebels")
            .WithVerb(HttpMethod.Put);
        Assert.Equal("some_rebels", rebels.Database);
    }

    [Fact]
    public async Task GetOrCreateDatabase_CustomCharacterName()
    {
        var databaseName = "rebel0_$()+/-";

        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        httpTest.RespondWithJson(new { ok = true });
        var rebels = await client.GetOrCreateDatabaseAsync<Rebel>(databaseName);
        httpTest
            .ShouldHaveCalled("http://localhost/rebel0_%24%28%29%2B%2F-")
            .WithVerb(HttpMethod.Put);
        Assert.Equal(databaseName, rebels.Database);
    }

    [Fact]
    public async Task GetOrCreateDatabase_402_ReturnDatabase()
    {
        // Operation result
        httpTest.RespondWith(string.Empty, 412);
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        var rebels = await client.GetOrCreateDatabaseAsync<Rebel>();

        Assert.NotNull(rebels);

        httpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithVerb(HttpMethod.Put);
        Assert.Equal("rebels", rebels.Database);
    }

    [Fact]
    public async Task GetOrCreateDatabase_InvalidCharacters_ThrowsArgumentException()
    {
        // Operation result
        httpTest.RespondWithJson(new { ok = true });
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        Func<Task> action = () => client.GetOrCreateDatabaseAsync<Rebel>("rebel.");
        var ex = await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Contains("invalid characters", ex.Message);
    }

    [Fact]
    public async Task CreateDatabaseAsync_Default()
    {
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        httpTest.RespondWithJson(new { ok = true });
        var rebels = await client.CreateDatabaseAsync<Rebel>();
        httpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithVerb(HttpMethod.Put);
        Assert.Equal("rebels", rebels.Database);
    }

    [Fact]
    public async Task CreateDatabase_402_ThrowsException()
    {
        // Operation result
        httpTest.RespondWith((string)null, 412);
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        Func<Task> action = () => client.CreateDatabaseAsync<Rebel>();
        await Assert.ThrowsAsync<CouchException>(action);
    }

    [Fact]
    public async Task CreateDatabaseAsync_Params()
    {
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        httpTest.RespondWithJson(new { ok = true });
        var rebels = await client.CreateDatabaseAsync<Rebel>(9, 2, true);
        httpTest
            .ShouldHaveCalled("http://localhost/rebels*")
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
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        httpTest.RespondWithJson(new { ok = true });
        var rebels = await client.CreateDatabaseAsync<Rebel>(8, 3, false);
        httpTest
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
        httpTest.RespondWithJson(new { ok = true });
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        await client.DeleteDatabaseAsync<Rebel>();
        httpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithVerb(HttpMethod.Delete);
    }

    [Fact]
    public async Task DeleteDatabase_CustomName()
    {
        // Operation result
        httpTest.RespondWithJson(new { ok = true });
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        await client.DeleteDatabaseAsync("some_rebels");
        httpTest
            .ShouldHaveCalled("http://localhost/some_rebels")
            .WithVerb(HttpMethod.Delete);
    }

    [Fact]
    public async Task DeleteDatabase_CustomCharacterName()
    {
        // Operation result
        httpTest.RespondWithJson(new { ok = true });
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        await client.DeleteDatabaseAsync("rebel0_$()+/-");
        httpTest
            .ShouldHaveCalled("http://localhost/rebel0_%24%28%29%2B%2F-")
            .WithVerb(HttpMethod.Delete);
    }

    [Fact]
    public async Task DeleteDatabase_InvalidCharacters_ThrowsArgumentException()
    {
        // Operation result
        httpTest.RespondWithJson(new { ok = true });
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
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
        httpTest.RespondWithJson(new { status = "ok" });
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        var db = "rebel";
        await using var client = TestCouchClientFactory.Create(httpTest);
        var result = await client.ExistsAsync(db);
        Assert.True(result);

        httpTest
            .ShouldHaveCalled($"http://localhost/{db}")
            .WithVerb(HttpMethod.Head);
    }

    [Fact]
    public async Task NotExists()
    {
        httpTest.RespondWith(string.Empty, 404);
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        var db = "rebel";
        await using var client = TestCouchClientFactory.Create(httpTest);
        var result = await client.ExistsAsync(db);
        Assert.False(result);

        httpTest
            .ShouldHaveCalled($"http://localhost/{db}")
            .WithVerb(HttpMethod.Head);
    }

    [Fact]
    public async Task IsUp()
    {
        // Operation result
        httpTest.RespondWithJson(new { status = "ok" });
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        var result = await client.IsUpAsync();
        Assert.True(result);

        httpTest
            .ShouldHaveCalled($"http://localhost/_up")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task IsNotUp()
    {
        httpTest.RespondWith(string.Empty, 404);
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        var result = await client.IsUpAsync();
        Assert.False(result);

        httpTest
            .ShouldHaveCalled($"http://localhost/_up")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task IsNotUp_Timeout()
    {
        httpTest.SimulateTimeout();
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        var result = await client.IsUpAsync();
        Assert.False(result);

        httpTest
            .ShouldHaveCalled($"http://localhost/_up")
            .WithVerb(HttpMethod.Get);
    }


    [Fact]
    public async Task DatabaseNames()
    {
        // Databases
        httpTest.RespondWithJson(new[] { "jedi", "sith" });
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        var dbs = await client.GetDatabasesNamesAsync();
        httpTest
            .ShouldHaveCalled("http://localhost/_all_dbs")
            .WithVerb(HttpMethod.Get);
        Assert.Equal(["jedi", "sith"], dbs);
    }

    [Fact]
    public async Task ActiveTasks()
    {
        // Tasks
        httpTest.RespondWithJson(new List<CouchActiveTask>());

        // Logout
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
        var dbs = await client.GetActiveTasksAsync();
        httpTest
            .ShouldHaveCalled("http://localhost/_active_tasks")
            .WithVerb(HttpMethod.Get);
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task ConflictException()
    {
        httpTest.RespondWith(string.Empty, (int)HttpStatusCode.Conflict);

        await using var client = TestCouchClientFactory.Create(httpTest);
        var couchException =
            await Assert.ThrowsAsync<CouchConflictException>(() => client.CreateDatabaseAsync<Rebel>());
        Assert.IsType<CouchHttpResponseException>(couchException.InnerException);
    }

    [Fact]
    public async Task NotFoundException()
    {
        httpTest.RespondWith(string.Empty, (int)HttpStatusCode.NotFound);

        await using var client = TestCouchClientFactory.Create(httpTest);
        var couchException =
            await Assert.ThrowsAsync<CouchNotFoundException>(() => client.DeleteDatabaseAsync<Rebel>());
        Assert.IsType<CouchHttpResponseException>(couchException.InnerException);
    }

    [Fact]
    public async Task BadRequestException()
    {
        httpTest.RespondWith(@"{error: ""no_usable_index""}", (int)HttpStatusCode.BadRequest);

        await using var client = TestCouchClientFactory.Create(httpTest);
        var db = client.GetDatabase<Rebel>();
        var couchException = Assert.Throws<CouchNoIndexException>(() => db.UseIndex("aoeu").ToList());
        Assert.IsType<CouchHttpResponseException>(couchException.InnerException);
    }

    [Fact]
    public async Task GenericExceptionWithMessage()
    {
        string message = "message text";
        string reason = "reason text";
        httpTest.RespondWith($"{{error: \"{message}\", reason: \"{reason}\"}}",
            (int)HttpStatusCode.InternalServerError);

        await using var client = TestCouchClientFactory.Create(httpTest);
        var db = client.GetDatabase<Rebel>();
        var couchException = await Assert.ThrowsAsync<CouchException>(() => db.CompactAsync());
        Assert.Equal(message, couchException.Message);
        Assert.Equal(reason, couchException.Reason);
        Assert.IsType<CouchHttpResponseException>(couchException.InnerException);
    }

    [Fact]
    public async Task GenericExceptionNoMessage()
    {
        httpTest.RespondWith(string.Empty, (int)HttpStatusCode.InternalServerError);

        await using var client = TestCouchClientFactory.Create(httpTest);
        var db = client.GetDatabase<Rebel>();
        var couchException = await Assert.ThrowsAsync<CouchException>(() => db.CompactAsync());
        Assert.IsType<CouchHttpResponseException>(couchException.InnerException);
    }

    #endregion
}