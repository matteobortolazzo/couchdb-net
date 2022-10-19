using CouchDB.Driver.Security;
using CouchDB.UnitTests.Models;
using Flurl.Http.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.UnitTests._Models;
using CouchDB.Driver.Views;
using Xunit;
using CouchDB.Driver.DatabaseApiMethodOptions;

namespace CouchDB.Driver.UnitTests
{
    public class Database_Tests : IAsyncDisposable
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
                _attachments = new Dictionary<string, object>
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

            Assert.NotNull(newR);
            Assert.NotEmpty(newR.Attachments);
            Assert.NotNull(newR.Attachments["luke.txt"].Uri);

        }

        [Fact]
        public async Task FindWithConflicts()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                _attachments = new Dictionary<string, object>
                {
                    { "luke.txt", new { ContentType = "text/plain" } }
                }
            });

            var newR = await _rebels.FindAsync("1", true);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/1*")
                .WithQueryParam("conflicts", "true")
                .WithVerb(HttpMethod.Get);
        }

        [Fact]
        public async Task FindWithOptionsRevision()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                _attachments = new Dictionary<string, object>
                {
                    { "luke.txt", new { ContentType = "text/plain" } }
                }
            });

            var newR = await _rebels.FindAsync("1", new FindOptions { Rev = "1-xxx" });
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/1*")
                .WithQueryParam("rev", "1-xxx")
                .WithVerb(HttpMethod.Get);
        }

        [Fact]
        public async Task FindWithOptionsConflicts()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                _attachments = new Dictionary<string, object>
                {
                    { "luke.txt", new { ContentType = "text/plain" } }
                }
            });

            var newR = await _rebels.FindAsync("1", new FindOptions { Conflicts = true });
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/1*")
                .WithQueryParam("conflicts", "true")
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
        public async Task FindManyWithNotFoundError()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWith(@"{""results"":[{""id"":""1"",""docs"":[{""error"":{""id"":""1"",""rev"":""undefined"",""error"":""not_found"",""reason"":""missing""}}]}]}");
            var ids = new string[] { "1" };
            var result = await _rebels.FindManyAsync(ids);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_bulk_get")
                .WithRequestJson(new
                {
                    docs = new[]
                    {
                            new { id = "1" },
                    }
                })
                .WithVerb(HttpMethod.Post);

            Assert.NotNull(result);
            Assert.Empty(result);
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
        public async Task CreateWithOptionsBatch()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "xxx" });

            var r = new Rebel { Name = "Luke" };
            var newR = await _rebels.AddAsync(r, new AddOptions { Batch = true });
            httpTest
                .ShouldHaveCalled("http://localhost/rebels")
                .WithQueryParam("batch", "ok")
                .WithVerb(HttpMethod.Post);
        }

        [Fact]
        public async Task CreateOrUpdate()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "xxx" });

            var r = new Rebel { Name = "Luke", Id = "1" };
            var newR = await _rebels.AddOrUpdateAsync(r);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/1")
                .WithVerb(HttpMethod.Put);
        }

        [Fact]
        public async Task CreateOrUpdateWithOptionsRevision()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "2-xxx" });

            var r = new Rebel { Name = "Luke", Id = "1" };
            var newR = await _rebels.AddOrUpdateAsync(r, new AddOrUpdateOptions { Rev = "1-xxx" });
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/1")
                .WithQueryParam("rev", "1-xxx")
                .WithVerb(HttpMethod.Put);
        }

        [Fact]
        public async Task CreateOrUpdateWithOptionsBatch()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "2-xxx" });

            var r = new Rebel { Name = "Luke", Id = "1" };
            var newR = await _rebels.AddOrUpdateAsync(r, new AddOrUpdateOptions { Batch = true });
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/1")
                .WithQueryParam("batch", "ok")
                .WithVerb(HttpMethod.Put);
        }

        [Fact]
        public async Task Create_Discriminator()
        {
            var rebels = _client.GetDatabase<Rebel>(database: "rebels", discriminator: "myRebels");
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "xxx" });

            var r = new Rebel { Name = "Luke" };
            var newR = await rebels.AddAsync(r);
            Assert.Equal("myRebels", newR.SplitDiscriminator);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels")
                .WithVerb(HttpMethod.Post);
        }

        [Fact]
        public async Task CreateOrUpdate_Discriminator()
        {
            var rebels = _client.GetDatabase<Rebel>(database: "rebels", discriminator: "myRebels");
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "xxx" });

            var r = new Rebel { Name = "Luke", Id = "1" };
            var newR = await rebels.AddOrUpdateAsync(r);
            Assert.Equal("myRebels", newR.SplitDiscriminator);
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
                await _rebels.AddOrUpdateAsync(r);
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
        
        [Fact]
        public async Task DeleteRange()
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
            }.Select(doc => (DocumentId)doc).ToArray();
            await _rebels.DeleteRangeAsync(moreRebels);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_bulk_docs")
                .WithVerb(HttpMethod.Post);
        }
        
        [Fact]
        public async Task DeleteRange_Docs()
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
            await _rebels.DeleteRangeAsync(moreRebels);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_bulk_docs")
                .WithVerb(HttpMethod.Post);
        }

        #endregion

        #region View

        [Fact]
        public async Task GetViewAsync_WithNoOptions_CallGet()
        {
            // Arrange
            using var httpTest = new HttpTest();
            SetupViewResponse(httpTest);

            // Act
            var rebels = await _rebels.GetViewAsync<string[], RebelView>("jedi", "by_name");

            // Assert
            var rebel = Assert.Single(rebels);
            Assert.Equal("luke", rebel.Id);
            Assert.Equal(new[] { "Luke", "Skywalker" }, rebel.Key);
            Assert.Equal(3, rebel.Value.NumberOfBattles);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_design/jedi/_view/by_name")
                .WithVerb(HttpMethod.Get);
        }

        [Fact]
        public async Task GetViewAsync_WithOptions_CallPost()
        {
            // Arrange
            using var httpTest = new HttpTest();
            SetupViewResponse(httpTest);
            var options = new CouchViewOptions<string[]>
            {
                Key = new[] {"Luke", "Skywalker"},
                Skip = 10
            };

            // Act
            var rebels = await _rebels.GetViewAsync<string[], RebelView>("jedi", "by_name", options);

            // Assert
            var rebel = Assert.Single(rebels);
            Assert.Equal("luke", rebel.Id);
            Assert.Equal(new[] { "Luke", "Skywalker" }, rebel.Key);
            Assert.Equal(3, rebel.Value.NumberOfBattles);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_design/jedi/_view/by_name")
                .WithVerb(HttpMethod.Post)
                .WithRequestBody(@"{""key"":[""Luke"",""Skywalker""],""skip"":10}");
        }

        [Fact]
        public async Task GetDetailed_WithNoOptions_CallGet()
        {
            // Arrange
            using var httpTest = new HttpTest();
            SetupViewResponse(httpTest);

            // Act
            var list = await _rebels.GetDetailedViewAsync<string[], RebelView>("jedi", "by_name");

            // Assert
            Assert.Equal(10, list.Offset);
            Assert.Equal(20, list.TotalRows);
            var rebel = Assert.Single(list.Rows);
            Assert.Equal("luke", rebel.Id);
            Assert.Equal(new[] { "Luke", "Skywalker" }, rebel.Key);
            Assert.Equal(3, rebel.Value.NumberOfBattles);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_design/jedi/_view/by_name")
                .WithVerb(HttpMethod.Get);
        }

        [Fact]
        public async Task GetDetailedViewAsync_WithOptions_CallPost()
        {
            // Arrange
            using var httpTest = new HttpTest();
            SetupViewResponse(httpTest);
            var options = new CouchViewOptions<string[]>
            {
                Key = new[] { "Luke", "Skywalker" },
                Update = UpdateStyle.Lazy
            };

            // Act
            var list = await _rebels.GetDetailedViewAsync<string[], RebelView>("jedi", "by_name", options);

            // Assert
            Assert.Equal(10, list.Offset);
            Assert.Equal(20, list.TotalRows);
            var rebel = Assert.Single(list.Rows);
            Assert.Equal("luke", rebel.Id);
            Assert.Equal(new[] { "Luke", "Skywalker" }, rebel.Key);
            Assert.Equal(3, rebel.Value.NumberOfBattles);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_design/jedi/_view/by_name")
                .WithVerb(HttpMethod.Post)
                .WithRequestBody(@"{""key"":[""Luke"",""Skywalker""],""update"":""lazy""}");
        }

        private static void SetupViewResponse(HttpTest httpTest)
        {
            httpTest.RespondWithJson(new
            {
                Offset = 10,
                Total_Rows = 20,
                Rows = new[]
                {
                    new
                    {
                        Id = "luke",
                        Key = new [] {"Luke", "Skywalker"},
                        Value = new
                        {
                            NumberOfBattles = 3
                        }
                    }
                }
            });
        }

        [Fact]
        public async Task GetViewQueryAsync()
        {
            // Arrange
            using var httpTest = new HttpTest();
            SetupViewQueryResponse(httpTest);
            var options = new CouchViewOptions<string[]>
            {
                Key = new[] {"Luke", "Skywalker"},
                Skip = 10
            };
            var queries = new[]
            {
                options,
                options
            };

            // Act
            var results = await _rebels.GetViewQueryAsync<string[], RebelView>("jedi", "by_name", queries);

            // Assert
            Assert.Equal(2, results.Length);

            Assert.All(results, result =>
            {
                var rebel = Assert.Single(result);
                Assert.Equal("luke", rebel.Id);
                Assert.Equal(new[] { "Luke", "Skywalker" }, rebel.Key);
                Assert.Equal(3, rebel.Value.NumberOfBattles);
            });
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_design/jedi/_view/by_name/queries")
                .WithVerb(HttpMethod.Post)
                .WithRequestBody(@"{""queries"":[{""key"":[""Luke"",""Skywalker""],""skip"":10},{""key"":[""Luke"",""Skywalker""],""skip"":10}]}");
        }

        [Fact]
        public async Task GetDetailedViewQueryAsync()
        {
            // Arrange
            using var httpTest = new HttpTest();
            SetupViewQueryResponse(httpTest);
            var options = new CouchViewOptions<string[]>
            {
                Key = new[] {"Luke", "Skywalker"},
                Skip = 10
            };
            var queries = new[]
            {
                options,
                options
            };

            // Act
            var results = await _rebels.GetDetailedViewQueryAsync<string[], RebelView>("jedi", "by_name", queries);

            // Assert
            Assert.Equal(2, results.Length);

            Assert.All(results, result =>
            {
                Assert.Equal(10, result.Offset);
                Assert.Equal(20, result.TotalRows);
                var rebel = Assert.Single(result.Rows);
                Assert.Equal("luke", rebel.Id);
                Assert.Equal(new[] { "Luke", "Skywalker" }, rebel.Key);
                Assert.Equal(3, rebel.Value.NumberOfBattles);
            });
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_design/jedi/_view/by_name/queries")
                .WithVerb(HttpMethod.Post)
                .WithRequestBody(@"{""queries"":[{""key"":[""Luke"",""Skywalker""],""skip"":10},{""key"":[""Luke"",""Skywalker""],""skip"":10}]}");
        }

        private static void SetupViewQueryResponse(HttpTest httpTest)
        {
            httpTest.RespondWithJson(new
            {
                Results = new[]
                {
                    new
                    {
                        Offset = 10,
                        Total_Rows = 20,
                        Rows = new[]
                        {
                            new
                            {
                                Id = "luke",
                                Key = new [] {"Luke", "Skywalker"},
                                Value = new
                                {
                                    NumberOfBattles = 3
                                }
                            }
                        }
                    },
                    new
                    {
                        Offset = 10,
                        Total_Rows = 20,
                        Rows = new[]
                        {
                            new
                            {
                                Id = "luke",
                                Key = new [] {"Luke", "Skywalker"},
                                Value = new
                                {
                                    NumberOfBattles = 3
                                }
                            }
                        }
                    }
                }
            });
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
