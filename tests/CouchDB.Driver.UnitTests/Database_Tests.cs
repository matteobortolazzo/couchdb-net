using CouchDB.Driver.Security;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Database_Tests: IAsyncDisposable
    {
        private readonly ICouchClient _client;
        private readonly ICouchDatabase<Rebel> _rebels;

        public Database_Tests()
        {
            _client = new CouchClient("http://localhost");
            _rebels = _client.GetDatabase<Rebel>();
        }

        #region Crud

        [Fact]
        public async Task Find()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                Attachments = new Dictionary<string, object>
                    {
                        { "luke.txt", new { ContentType = "text/plain" } }
                    }
            });

            var a = new List<Rebel>();
            var newR = await _rebels.FindAsync("1");
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/1")
                .WithoutQueryParam("conflicts")
                .WithVerb(HttpMethod.Get);
        }

        [Fact]
        public async Task FindWithConflicts()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                Attachments = new Dictionary<string, object>
                    {
                        { "luke.txt", new { ContentType = "text/plain" } }
                    }
            });

            var newR = await _rebels.FindAsync("1", true);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/1")
                .WithQueryParamValue("conflicts", true)
                .WithVerb(HttpMethod.Get);
        }

        [Fact]
        public async Task FindMany()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWith(@"{""results"":[{""id"":""1"",""docs"":[{""ok"":{""_id"":""1"",""Name"":""Luke""}}]},{""id"":""2"",""docs"":[{""ok"":{""_id"":""2"",""Name"":""Leia""}}]}]}");
            var ids = new string[] { "1", "2" };
            var result = await _rebels.FindManyAsync(ids);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_bulk_get")
                .WithRequestJson(new
                {
                    docs = new[]
                    {
                            new { id = "1" },
                            new { id = "2" },
                    }
                })
                .WithVerb(HttpMethod.Post);

            Assert.Equal(2, result.Count);
            Assert.Equal("Luke", result[0].Name);
            Assert.Equal("Leia", result[1].Name);
        }

        [Fact]
        public async Task Create()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "xxx" });

            var r = new Rebel { Name = "Luke" };
            var newR = await _rebels.AddAsync(r);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels")
                .WithVerb(HttpMethod.Post);
        }

        [Fact]
        public async Task CreateOrUpdate()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "xxx" });

            var r = new Rebel { Name = "Luke", Id = "1" };
            var newR = await _rebels.AddOrUpdate(r);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/1")
                .WithVerb(HttpMethod.Put);
        }

        [Fact]
        public async Task CreateOrUpdate_WithoutId()
        {
            using var httpTest = new HttpTest();
            var exception = await Record.ExceptionAsync(async () =>
            {
                var r = new Rebel { Name = "Luke" };
                await _rebels.AddOrUpdate(r);
            });
            Assert.NotNull(exception);
            Assert.IsType<InvalidOperationException>(exception);
        }

        [Fact]
        public async Task Delete()
        {
            using var httpTest = new HttpTest();
            // Operation response
            httpTest.RespondWithJson(new { ok = true });

            var r = new Rebel { Name = "Luke", Id = "1", Rev = "1" };
            await _rebels.RemoveAsync(r);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/1?rev=1")
                .WithVerb(HttpMethod.Delete);
        }

        [Fact]
        public async Task CouchList()
        {
            using var httpTest = new HttpTest();
            // ToList
            httpTest.RespondWithJson(new { Docs = new List<string>(), Bookmark = "bookmark" });
            // Operation response
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            var rebels = client.GetDatabase<Rebel>();
            var completeResult = await rebels.ToCouchListAsync();

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_find")
                .WithVerb(HttpMethod.Post);
            Assert.Equal("bookmark", completeResult.Bookmark);
        }

        [Fact]
        public async Task QueryJson()
        {
            using var httpTest = new HttpTest();
            var expected = new List<Rebel>() { new Rebel { Id = Guid.NewGuid().ToString() } };
            httpTest.RespondWithJson(new { Docs = expected });

            var query = @"{""selector"":{""age"":19}}";
            var result = await _rebels.QueryAsync(query);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_find")
                .WithVerb(HttpMethod.Post)
                .WithRequestBody(@"{""selector"":{""age"":19}}");
            Assert.Equal(expected.Count, result.Count);
            Assert.Equal(expected[0].Id, result[0].Id);
        }

        [Fact]
        public async Task QueryObject()
        {
            using var httpTest = new HttpTest();
            var expected = new List<Rebel>() { new Rebel { Id = Guid.NewGuid().ToString() } };
            httpTest.RespondWithJson(new { Docs = expected });

            var query = new { selector = new { age = 19 } };
            var result = await _rebels.QueryAsync(query);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_find")
                .WithVerb(HttpMethod.Post)
                .WithRequestBody(@"{""selector"":{""age"":19}}");
            Assert.Equal(expected.Count, result.Count);
            Assert.Equal(expected[0].Id, result[0].Id);
        }

        #endregion

        #region Bulk

        [Fact]
        public async Task AddOrUpdateRange()
        {
            using var httpTest = new HttpTest();
            // Response
            httpTest.RespondWithJson(new[] {
                    new { Id = "111", Ok = true, Rev = "111" },
                    new { Id = "222", Ok = true, Rev = "222" },
                });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            var moreRebels = new[] {
                    new Rebel { Name = "Luke", Id = "1" },
                    new Rebel { Name = "Leia", Id = "2" }
                };
            var newR = await _rebels.AddOrUpdateRangeAsync(moreRebels);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_bulk_docs")
                .WithVerb(HttpMethod.Post);
        }

        #endregion

        #region Utils

        [Fact]
        public async Task Info()
        {
            using var httpTest = new HttpTest();
            await _rebels.GetInfoAsync();
            httpTest
                .ShouldHaveCalled("http://localhost/rebels")
                .WithVerb(HttpMethod.Get);
        }

        [Fact]
        public async Task Compact()
        {
            using var httpTest = new HttpTest();
            // Operation response
            httpTest.RespondWithJson(new { ok = true });

            await _rebels.CompactAsync();
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_compact")
                .WithVerb(HttpMethod.Post);
        }

        [Fact]
        public async Task SecurityInfo_Get()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                Admins = new
                {
                    Names = new[] { "superuser" },
                    Roles = new[] { "admins" }
                },
                Members = new
                {
                    Names = new[] { "user1", "user2" },
                    Roles = new[] { "developers" }
                }
            });
            var securityInfo = await _rebels.Security.GetInfoAsync();
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_security")
                .WithVerb(HttpMethod.Get);
            Assert.Equal("user1", securityInfo.Members.Names[0]);
        }

        [Fact]
        public async Task SecurityInfo_Put()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { ok = true });

            var securityInfo = new CouchSecurityInfo();
            securityInfo.Admins.Names.Add("user1");

            await _rebels.Security.SetInfoAsync(securityInfo);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_security")
                .WithVerb(HttpMethod.Put)
                .WithRequestJson(securityInfo);
        }

        #endregion

        public ValueTask DisposeAsync()
        {
            return _client.DisposeAsync();
        }
    }
}
