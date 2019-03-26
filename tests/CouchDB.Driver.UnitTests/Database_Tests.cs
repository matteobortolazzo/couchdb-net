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
    public class Database_Tests
    {
        private readonly CouchDatabase<Rebel> _rebels;

        public Database_Tests()
        {
            var client = new CouchClient("http://localhost:5984");
            _rebels = client.GetDatabase<Rebel>();
        }

        #region Crud

        [Fact]
        public async Task Find()
        {
            using (var httpTest = new HttpTest())
            {
                var newR = await _rebels.FindAsync("1");
                httpTest
                    .ShouldHaveCalled("http://localhost:5984/rebels/doc/1")
                    .WithVerb(HttpMethod.Get);
            }
        }
        [Fact]
        public async Task Add()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "xxx" });

                var r = new Rebel { Name = "Luke" };
                var newR = await _rebels.AddAsync(r);
                httpTest
                    .ShouldHaveCalled("http://localhost:5984/rebels")
                    .WithVerb(HttpMethod.Post);
            }
        }
        [Fact]
        public async Task AddOrUpdate()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "xxx" });

                var r = new Rebel { Name = "Luke", Id = "1" };
                var newR = await _rebels.AddOrUpdateAsync(r);
                httpTest
                    .ShouldHaveCalled("http://localhost:5984/rebels/doc/1")
                    .WithVerb(HttpMethod.Put);
            }
        }
        [Fact]
        public async Task AddOrUpdate_WithoutId()
        {
            using (var httpTest = new HttpTest())
            {
                var exception = await Record.ExceptionAsync(async () =>
                {
                    var r = new Rebel { Name = "Luke" };
                    await _rebels.AddOrUpdateAsync(r);
                });
                Assert.NotNull(exception);
                Assert.IsType<InvalidOperationException>(exception);
            }
        }
        [Fact]
        public async Task Delete()
        {
            using (var httpTest = new HttpTest())
            {
                var r = new Rebel { Name = "Luke", Id = "1", Rev = "1" };
                await _rebels.RemoveAsync(r);
                httpTest
                    .ShouldHaveCalled("http://localhost:5984/rebels/doc/1?rev=1")
                    .WithVerb(HttpMethod.Delete);
            }
        }

        #endregion

        #region Bulk

        [Fact]
        public async Task AddOrUpdateRange()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new [] {
                    new { Id = "111", Ok = true, Rev = "111" },
                    new { Id = "222", Ok = true, Rev = "222" },
                });

                var moreRebels = new[] {
                    new Rebel { Name = "Luke", Id = "1" },
                    new Rebel { Name = "Leia", Id = "2" }
                };
                var newR = await _rebels.AddOrUpdateRangeAsync(moreRebels);
                httpTest
                    .ShouldHaveCalled("http://localhost:5984/rebels/_bulk_docs")
                    .WithVerb(HttpMethod.Post);
            }
        }

        #endregion

        #region Utils

        [Fact]
        public async Task Info()
        {
            using (var httpTest = new HttpTest())
            {
                await _rebels.GetInfoAsync();
                httpTest
                    .ShouldHaveCalled("http://localhost:5984/rebels")
                    .WithVerb(HttpMethod.Get);
            }
        }
        [Fact]
        public async Task Compact()
        {
            using (var httpTest = new HttpTest())
            {
                await _rebels.CompactAsync();
                httpTest
                    .ShouldHaveCalled("http://localhost:5984/rebels/_compact")
                    .WithVerb(HttpMethod.Post);
            }
        }

        #endregion
    }
}
