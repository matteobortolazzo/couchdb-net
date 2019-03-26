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
    public class Client_Tests : IDisposable
    {
        private readonly CouchClient _client;

        public Client_Tests()
        {
            _client = new CouchClient("http://localhost:5984");
        }

        #region Add

        [Fact]
        public async Task AddDatabase_Default()
        {
            using (var httpTest = new HttpTest())
            {
                var rebels = await _client.AddDatabaseAsync<Rebel>();
                httpTest
                    .ShouldHaveCalled("http://localhost:5984/rebels")
                    .WithVerb(HttpMethod.Put);
                Assert.Equal("rebels", rebels.Database);
            }
        }
        [Fact]
        public async Task AddDatabase_CustomName()
        {
            using (var httpTest = new HttpTest())
            {
                var rebels = await _client.AddDatabaseAsync<Rebel>("some_rebels");
                httpTest
                    .ShouldHaveCalled("http://localhost:5984/some_rebels")
                    .WithVerb(HttpMethod.Put);
                Assert.Equal("some_rebels", rebels.Database);
            }
        }

        #endregion

        #region Delete

        [Fact]
        public async Task DeleteDatabase_Default()
        {
            using (var httpTest = new HttpTest())
            {
                await _client.RemoveDatabaseAsync<Rebel>();
                httpTest
                    .ShouldHaveCalled("http://localhost:5984/rebels")
                    .WithVerb(HttpMethod.Delete);
            }
        }
        [Fact]
        public async Task DeleteDatabase_CustomName()
        {
            using (var httpTest = new HttpTest())
            {
                await _client.RemoveDatabaseAsync<Rebel>("some_rebels");
                httpTest
                    .ShouldHaveCalled("http://localhost:5984/some_rebels")
                    .WithVerb(HttpMethod.Delete);
            }
        }

        #endregion

        #region Utils

        [Fact]
        public async Task DatabaseNames()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new[] { "jedi", "sith" });
                var dbs = await _client.GetDatabasesNamesAsync();
                httpTest
                    .ShouldHaveCalled("http://localhost:5984/_all_dbs")
                    .WithVerb(HttpMethod.Get);
                Assert.Equal(new[] { "jedi", "sith" }, dbs);
            }
        }
        [Fact]
        public async Task ActiveTasks()
        {
            using (var httpTest = new HttpTest())
            {
                var dbs = await _client.GetActiveTasksAsync();
                httpTest
                    .ShouldHaveCalled("http://localhost:5984/_active_tasks")
                    .WithVerb(HttpMethod.Get);
            }
        }

        #endregion

        #region Implementations

        public void Dispose()
        {
            _client.Dispose();
        }

        #endregion
    }
}
