using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
                using (var client = new CouchClient("http://localhost"))
                {
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
                using (var client = new CouchClient("http://localhost"))
                {
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
        public async Task DatabaseNames()
        {
            using (var httpTest = new HttpTest())
            {
                using (var client = new CouchClient("http://localhost"))
                {
                    httpTest.RespondWithJson(new[] { "jedi", "sith" });
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
