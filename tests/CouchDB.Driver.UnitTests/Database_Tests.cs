using CouchDB.Driver.Security;
using CouchDB.Driver.Types;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Database_Tests
    {
        private readonly CouchDatabase<Rebel> _rebels;

        public Database_Tests()
        {
            var client = new CouchClient("http://localhost");
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
                    .ShouldHaveCalled("http://localhost/rebels/1")
                    .WithoutQueryParam("conflicts")
                    .WithVerb(HttpMethod.Get);
            }
        }
        [Fact]
        public async Task FindWithConflicts()
        {
            using (var httpTest = new HttpTest())
            {
                var newR = await _rebels.FindAsync("1", true);
                httpTest
                    .ShouldHaveCalled("http://localhost/rebels/1")
                    .WithQueryParamValue("conflicts", true)
                    .WithVerb(HttpMethod.Get);
            }
        }
        [Fact]
        public async Task Create()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "xxx" });

                var r = new Rebel { Name = "Luke" };
                var newR = await _rebels.CreateAsync(r);
                httpTest
                    .ShouldHaveCalled("http://localhost/rebels")
                    .WithVerb(HttpMethod.Post);
            }
        }
        [Fact]
        public async Task CreateOrUpdate()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "xxx" });

                var r = new Rebel { Name = "Luke", Id = "1" };
                var newR = await _rebels.CreateOrUpdateAsync(r);
                httpTest
                    .ShouldHaveCalled("http://localhost/rebels/1")
                    .WithVerb(HttpMethod.Put);
            }
        }
        [Fact]
        public async Task CreateOrUpdate_WithoutId()
        {
            using (var httpTest = new HttpTest())
            {
                var exception = await Record.ExceptionAsync(async () =>
                {
                    var r = new Rebel { Name = "Luke" };
                    await _rebels.CreateOrUpdateAsync(r);
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
                await _rebels.DeleteAsync(r);
                httpTest
                    .ShouldHaveCalled("http://localhost/rebels/1?rev=1")
                    .WithVerb(HttpMethod.Delete);
            }
        }
        [Fact]
        public async Task CouchList()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new { Docs = new string[0], Bookmark = "bookmark" });

                using (var client = new CouchClient("http://localhost"))
                {
                    var rebels = client.GetDatabase<Rebel>();
                    var completeResult = await rebels.ToCouchListAsync();

                    httpTest
                        .ShouldHaveCalled("http://localhost/rebels/_find")
                        .WithVerb(HttpMethod.Post);
                    Assert.Equal("bookmark", completeResult.Bookmark);
                }
            }
        }

        #endregion

        #region Bulk

        [Fact]
        public async Task AddOrUpdateRange()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new[] {
                    new { Id = "111", Ok = true, Rev = "111" },
                    new { Id = "222", Ok = true, Rev = "222" },
                });

                var moreRebels = new[] {
                    new Rebel { Name = "Luke", Id = "1" },
                    new Rebel { Name = "Leia", Id = "2" }
                };
                var newR = await _rebels.CreateOrUpdateRangeAsync(moreRebels);
                httpTest
                    .ShouldHaveCalled("http://localhost/rebels/_bulk_docs")
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
                    .ShouldHaveCalled("http://localhost/rebels")
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
                    .ShouldHaveCalled("http://localhost/rebels/_compact")
                    .WithVerb(HttpMethod.Post);
            }
        }
        [Fact]
        public async Task SecurityInfo_Get()
        {
            using (var httpTest = new HttpTest())
            {
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
        }
        [Fact]
        public async Task SecurityInfo_Put()
        {
            using (var httpTest = new HttpTest())
            {
                var securityInfo = new CouchSecurityInfo();
                securityInfo.Admins.Names.Add("user1");

                await _rebels.Security.SetInfoAsync(securityInfo);
                httpTest
                    .ShouldHaveCalled("http://localhost/rebels/_security")
                    .WithVerb(HttpMethod.Put)
                    .WithRequestJson(securityInfo);
            }
        }

        #endregion
    }
}
