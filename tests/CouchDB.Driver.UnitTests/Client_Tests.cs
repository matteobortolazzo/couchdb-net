using CouchDB.Driver.Types;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Client_Tests
    {
        #region Create

        [Fact]
        public async Task CreateDatabase_Default()
        {
            using (var httpTest = new HttpTest())
            {
                // Logout
                httpTest.RespondWithJson(new { ok = true });

                using (var client = new CouchClient("http://localhost"))
                {
                    httpTest.RespondWithJson(new { ok = true });
                    var rebels = await client.CreateDatabaseAsync<Rebel>();
                    httpTest
                        .ShouldHaveCalled("http://localhost/rebels")
                        .WithVerb(HttpMethod.Put);
                    Assert.Equal("rebels", rebels.Database);
                }
            }
        }
        [Fact]
        public async Task CreateDatabase_CustomName()
        {
            using (var httpTest = new HttpTest())
            {
                // Logout
                httpTest.RespondWithJson(new { ok = true });

                using (var client = new CouchClient("http://localhost"))
                {
                    httpTest.RespondWithJson(new { ok = true });
                    var rebels = await client.CreateDatabaseAsync<Rebel>("some_rebels");
                    httpTest
                        .ShouldHaveCalled("http://localhost/some_rebels")
                        .WithVerb(HttpMethod.Put);
                    Assert.Equal("some_rebels", rebels.Database);
                }
            }
        }

        #endregion

        #region Delete

        [Fact]
        public async Task DeleteDatabase_Default()
        {
            using (var httpTest = new HttpTest())
            {
                // Operation result
                httpTest.RespondWithJson(new { ok = true });
                // Logout
                httpTest.RespondWithJson(new { ok = true });

                using (var client = new CouchClient("http://localhost"))
                {
                    await client.DeleteDatabaseAsync<Rebel>();
                    httpTest
                        .ShouldHaveCalled("http://localhost/rebels")
                        .WithVerb(HttpMethod.Delete);
                }
            }
        }
        [Fact]
        public async Task DeleteDatabase_CustomName()
        {
            using (var httpTest = new HttpTest())
            {
                // Operation result
                httpTest.RespondWithJson(new { ok = true });
                // Logout
                httpTest.RespondWithJson(new { ok = true });

                using (var client = new CouchClient("http://localhost"))
                {
                    await client.DeleteDatabaseAsync<Rebel>("some_rebels");
                    httpTest
                        .ShouldHaveCalled("http://localhost/some_rebels")
                        .WithVerb(HttpMethod.Delete);
                }
            }
        }

        #endregion

        #region Utils

        [Fact]
        public async Task IsUp()
        {
            using (var httpTest = new HttpTest())
            {
                // Operation result
                httpTest.RespondWithJson(new { status = "ok" });
                // Logout
                httpTest.RespondWithJson(new { ok = true });

                using (var client = new CouchClient("http://localhost"))
                {
                    var result = await client.IsUpAsync();                    
                    Assert.True(result);
                }
            }
        }
        [Fact]
        public async Task IsNotUp()
        {
            using (var httpTest = new HttpTest())
            {
                // Operation result
                httpTest.RespondWithJson(new { ok = true });
                // Logout
                httpTest.RespondWithJson(new { ok = true });

                using (var client = new CouchClient("http://localhost"))
                {
                    httpTest.RespondWith("Not found",  404);
                    var result = await client.IsUpAsync();
                    Assert.False(result);
                }
            }
        }

        [Fact]
        public async Task DatabaseNames()
        {
            using (var httpTest = new HttpTest())
            {
                // Databases
                httpTest.RespondWithJson(new[] { "jedi", "sith" });
                // Logout
                httpTest.RespondWithJson(new { ok = true });

                using (var client = new CouchClient("http://localhost"))
                {
                    var dbs = await client.GetDatabasesNamesAsync();
                    httpTest
                        .ShouldHaveCalled("http://localhost/_all_dbs")
                        .WithVerb(HttpMethod.Get);
                    Assert.Equal(new[] { "jedi", "sith" }, dbs);
                }
            }
        }
        [Fact]
        public async Task ActiveTasks()
        {
            using (var httpTest = new HttpTest())
            {
                // Tasks
                httpTest.RespondWithJson(new List<CouchActiveTask>());
                // Logout
                httpTest.RespondWithJson(new { ok = true });

                using (var client = new CouchClient("http://localhost"))
                {
                    var dbs = await client.GetActiveTasksAsync();
                    httpTest
                        .ShouldHaveCalled("http://localhost/_active_tasks")
                        .WithVerb(HttpMethod.Get);
                }
            }
        }

        #endregion
    }
}
