using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Types;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CouchDB.Driver.Query.Extensions;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Client_Tests
    {
        #region Get

        [Fact]
        public async Task GetDatabase_CustomCharacterName()
        {
            var databaseName = "rebel0_$()+/-";

            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            httpTest.RespondWithJson(new { ok = true });
            var rebels = client.GetDatabase<Rebel>(databaseName);
            Assert.Equal(databaseName, rebels.Database);
        }

        [Fact]
        public async Task GetDatabase_InvalidCharacters_ThrowsArgumentException()
        {
            using var httpTest = new HttpTest();
            // Operation result
            httpTest.RespondWithJson(new { ok = true });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            Action action = () => client.GetDatabase<Rebel>("rebel.");
            var ex = Assert.Throws<ArgumentException>(action);
            Assert.Contains("invalid characters", ex.Message);
        }

        #endregion

        #region Create

        [Fact]
        public async Task CreateDatabase_Default()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            httpTest.RespondWithJson(new { ok = true });
            var rebels = await client.CreateDatabaseAsync<Rebel>();
            httpTest
                .ShouldHaveCalled("http://localhost/rebels")
                .WithVerb(HttpMethod.Put);
            Assert.Equal("rebels", rebels.Database);
        }

        [Fact]
        public async Task CreateDatabase_CustomName()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            httpTest.RespondWithJson(new { ok = true });
            var rebels = await client.CreateDatabaseAsync<Rebel>("some_rebels");
            httpTest
                .ShouldHaveCalled("http://localhost/some_rebels")
                .WithVerb(HttpMethod.Put);
            Assert.Equal("some_rebels", rebels.Database);
        }

        [Fact]
        public async Task CreateDatabase_CustomCharacterName()
        {
            var databaseName = "rebel0_$()+/-";

            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            httpTest.RespondWithJson(new { ok = true });
            var rebels = await client.CreateDatabaseAsync<Rebel>(databaseName);
            httpTest
                .ShouldHaveCalled("http://localhost/rebel0_%24%28%29%2B%2F-")
                .WithVerb(HttpMethod.Put);
            Assert.Equal(databaseName, rebels.Database);
        }

        [Fact]
        public async Task CreateDatabase_402_ReturnDatabase()
        {
            using var httpTest = new HttpTest();
            // Operation result
            httpTest.RespondWith((HttpContent)null, 412);
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            var rebels = await client.CreateDatabaseAsync<Rebel>();

            Assert.NotNull(rebels);

            httpTest
                .ShouldHaveCalled("http://localhost/rebels")
                .WithVerb(HttpMethod.Put);
            Assert.Equal("rebels", rebels.Database);
        }

        [Fact]
        public async Task CreateDatabase_InvalidCharacters_ThrowsArgumentException()
        {
            using var httpTest = new HttpTest();
            // Operation result
            httpTest.RespondWithJson(new { ok = true });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            Func<Task> action = () => client.CreateDatabaseAsync<Rebel>("rebel.");
            var ex = await Assert.ThrowsAsync<ArgumentException>(action);
            Assert.Contains("invalid characters", ex.Message);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task DeleteDatabase_Default()
        {
            using var httpTest = new HttpTest();
            // Operation result
            httpTest.RespondWithJson(new { ok = true });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            await client.DeleteDatabaseAsync<Rebel>();
            httpTest
                .ShouldHaveCalled("http://localhost/rebels")
                .WithVerb(HttpMethod.Delete);
        }

        [Fact]
        public async Task DeleteDatabase_CustomName()
        {
            using var httpTest = new HttpTest();
            // Operation result
            httpTest.RespondWithJson(new { ok = true });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            await client.DeleteDatabaseAsync<Rebel>("some_rebels");
            httpTest
                .ShouldHaveCalled("http://localhost/some_rebels")
                .WithVerb(HttpMethod.Delete);
        }

        [Fact]
        public async Task DeleteDatabase_CustomCharacterName()
        {
            using var httpTest = new HttpTest();
            // Operation result
            httpTest.RespondWithJson(new { ok = true });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            await client.DeleteDatabaseAsync<Rebel>("rebel0_$()+/-");
            httpTest
                .ShouldHaveCalled("http://localhost/rebel0_%24%28%29%2B%2F-")
                .WithVerb(HttpMethod.Delete);
        }

        [Fact]
        public async Task DeleteDatabase_InvalidCharacters_ThrowsArgumentException()
        {
            using var httpTest = new HttpTest();
            // Operation result
            httpTest.RespondWithJson(new { ok = true });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            Func<Task> action = () => client.DeleteDatabaseAsync<Rebel>("rebel.");
            var ex = await Assert.ThrowsAsync<ArgumentException>(action);
            Assert.Contains("invalid characters", ex.Message);
        }

        #endregion

        #region Utils

        [Fact]
        public async Task Exists()
        {
            using var httpTest = new HttpTest();
            // Operation result
            httpTest.RespondWithJson(new { status = "ok" });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            var db = Guid.NewGuid().ToString();
            await using var client = new CouchClient("http://localhost");
            var result = await client.ExistsAsync(db);
            Assert.True(result);

            httpTest
                .ShouldHaveCalled($"http://localhost/{db}")
                .WithVerb(HttpMethod.Head);
        }

        [Fact]
        public async Task NotExists()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWith((HttpContent)null, 404);
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            var db = Guid.NewGuid().ToString();
            await using var client = new CouchClient("http://localhost");
            var result = await client.ExistsAsync(db);
            Assert.False(result);

            httpTest
                .ShouldHaveCalled($"http://localhost/{db}")
                .WithVerb(HttpMethod.Head);
        }

        [Fact]
        public async Task IsUp()
        {
            using var httpTest = new HttpTest();
            // Operation result
            httpTest.RespondWithJson(new { status = "ok" });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            var result = await client.IsUpAsync();
            Assert.True(result);

            httpTest
                .ShouldHaveCalled($"http://localhost/_up")
                .WithVerb(HttpMethod.Get);
        }

        [Fact]
        public async Task IsNotUp()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWith((HttpContent)null, 404);
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            var result = await client.IsUpAsync();
            Assert.False(result);

            httpTest
                .ShouldHaveCalled($"http://localhost/_up")
                .WithVerb(HttpMethod.Get);
        }

        [Fact]
        public async Task DatabaseNames()
        {
            using var httpTest = new HttpTest();
            // Databases
            httpTest.RespondWithJson(new[] { "jedi", "sith" });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            var dbs = await client.GetDatabasesNamesAsync();
            httpTest
                .ShouldHaveCalled("http://localhost/_all_dbs")
                .WithVerb(HttpMethod.Get);
            Assert.Equal(new[] { "jedi", "sith" }, dbs);
        }

        [Fact]
        public async Task ActiveTasks()
        {
            using var httpTest = new HttpTest();
            // Tasks
            httpTest.RespondWithJson(new List<CouchActiveTask>());
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
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
            using var httpTest = new HttpTest();
            httpTest.RespondWith(status: (int)HttpStatusCode.Conflict);

            await using var client = new CouchClient("http://localhost");
            var couchException = await Assert.ThrowsAsync<CouchConflictException>(() => client.CreateDatabaseAsync<Rebel>());
            Assert.IsType<Flurl.Http.FlurlHttpException>(couchException.InnerException);
        }

        [Fact]
        public async Task NotFoundException()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWith(status: (int)HttpStatusCode.NotFound);

            await using var client = new CouchClient("http://localhost");
            var couchException = await Assert.ThrowsAsync<CouchNotFoundException>(() => client.DeleteDatabaseAsync<Rebel>());
            Assert.IsType<Flurl.Http.FlurlHttpException>(couchException.InnerException);
        }

        [Fact]
        public async Task BadRequestException()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWith(@"{error: ""no_usable_index""}", (int)HttpStatusCode.BadRequest);

            await using var client = new CouchClient("http://localhost");
            var db = client.GetDatabase<Rebel>();
            var couchException = Assert.Throws<CouchNoIndexException>(() => db.UseIndex("aoeu").ToList());
            Assert.IsType<Flurl.Http.FlurlHttpException>(couchException.InnerException);
        }

        [Fact]
        public async Task GenericExceptionWithMessage()
        {
            using var httpTest = new HttpTest();
            string message = "message text";
            string reason = "reason text";
            httpTest.RespondWith($"{{error: \"{message}\", reason: \"{reason}\"}}", (int)HttpStatusCode.InternalServerError);

            await using var client = new CouchClient("http://localhost");
            var db = client.GetDatabase<Rebel>();
            var couchException = await Assert.ThrowsAsync<CouchException>(() => db.FindAsync("aoeu"));
            Assert.Equal(message, couchException.Message);
            Assert.Equal(reason, couchException.Reason);
            Assert.IsType<Flurl.Http.FlurlHttpException>(couchException.InnerException);
        }

        [Fact]
        public async Task GenericExceptionNoMessage()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWith(status: (int)HttpStatusCode.InternalServerError);

            await using var client = new CouchClient("http://localhost");
            var db = client.GetDatabase<Rebel>();
            var couchException = await Assert.ThrowsAsync<CouchException>(() => db.FindAsync("aoeu"));
            Assert.IsType<Flurl.Http.FlurlHttpException>(couchException.InnerException);
        }

        #endregion
    }
}
