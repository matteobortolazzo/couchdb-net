using CouchDB.Driver.Security;
using CouchDB.UnitTests.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.UnitTests._Models;
using CouchDB.Driver.Views;
using Xunit;
using CouchDB.Driver.Types;
using CouchDB.Driver.UnitTests._Helpers;

namespace CouchDB.Driver.UnitTests;

public class Database_Tests : HttpTests
{
    #region Crud

    [Fact]
    public async Task ReadItem()
    {
        HttpTest.RespondWithJson(new
        {
            _attachments = new Dictionary<string, object>
            {
                { "luke.txt", new { ContentType = "text/plain" } }
            }
        });

        var newR = await Rebels.ReadItemAsync("1");
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/1")
            .WithoutQueryParam("conflicts")
            .WithVerb(HttpMethod.Get);

        Assert.NotNull(newR);
        Assert.NotNull(newR.Attachments);
        Assert.NotEmpty(newR.Attachments);
    }

    [Fact]
    public async Task ReadItemWithConflicts()
    {
        HttpTest.RespondWithJson(new
        {
            _attachments = new Dictionary<string, object>
            {
                { "luke.txt", new { ContentType = "text/plain" } }
            }
        });

        await Rebels.ReadItemAsync("1", new ReadItemOptions
        {
            Conflicts = true
        });
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/1")
            .WithQueryParam("conflicts", "true")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task ReadItemWithOptionsRevision()
    {
        HttpTest.RespondWithJson(new
        {
            _attachments = new Dictionary<string, object>
            {
                { "luke.txt", new { ContentType = "text/plain" } }
            }
        });

        await Rebels.ReadItemAsync("1", new ReadItemOptions { Revision = "1-xxx" });
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/1")
            .WithQueryParam("rev", "1-xxx")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task ReadItemWithOptionsConflicts()
    {
        HttpTest.RespondWithJson(new
        {
            _attachments = new Dictionary<string, object>
            {
                { "luke.txt", new { ContentType = "text/plain" } }
            }
        });

        await Rebels.ReadItemAsync("1", new ReadItemOptions { Conflicts = true });
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/1")
            .WithQueryParam("conflicts", "true")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task ReadItems()
    {
        HttpTest.RespondWith(
            @"{""results"":[{""id"":""1"",""docs"":[{""ok"":{""_id"":""1"",""Name"":""Luke""}}]},{""id"":""2"",""docs"":[{""ok"":{""_id"":""2"",""Name"":""Leia""}}]}]}");
        var ids = new[] { "1", "2" };
        var result = await Rebels.ReadItemsAsync(ids);
        HttpTest
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
    public async Task ReadItemsWithNotFoundError()
    {
        HttpTest.RespondWith(
            @"{""results"":[{""id"":""1"",""docs"":[{""error"":{""id"":""1"",""rev"":""undefined"",""error"":""not_found"",""reason"":""missing""}}]}]}");
        var ids = new[] { "1" };
        var result = await Rebels.ReadItemsAsync(ids);
        HttpTest
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
        HttpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "xxx" });

        var r = new Rebel { Name = "Luke" };
        await Rebels.CreateItemAsync(r);
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task CreateWithOptionsBatch()
    {
        HttpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "xxx" });

        var r = new Rebel { Name = "Luke" };
        await Rebels.CreateItemAsync(r, new CreateItemRequestOptions
        {
            Batch = true
        });
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithQueryParam("batch", "ok")
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task CreateWithOptionsRevision()
    {
        HttpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "2-xxx" });

        var r = new Rebel { Name = "Luke", Id = "1" };
        await Rebels.UpdateItemAsync(r, r.Id, "1-xxx");
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/1")
            .WithHeader("If-Match", "1-xxx")
            .WithVerb(HttpMethod.Put);
    }

    [Fact]
    public async Task Delete()
    {
        // Operation response
        HttpTest.RespondWithJson(new { ok = true });

        await Rebels.DeleteItemAsync("1", "1");
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/1")
            .WithHeader("If-Match", "1")
            .WithVerb(HttpMethod.Delete);
    }

    [Fact]
    public async Task CouchList()
    {
        // ToList
        HttpTest.RespondWithJson(new { Docs = new List<string>(), Bookmark = "bookmark" });
        // Operation response
        HttpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(HttpTest);
        var rebels = client.GetDatabase<Rebel>();
        var completeResult = await rebels.ToCouchListAsync();

        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_find")
            .WithVerb(HttpMethod.Post);
        Assert.Equal("bookmark", completeResult.Bookmark);
    }

    [Fact]
    public async Task QueryJson()
    {
        var expected = new List<Rebel>() { new Rebel { Id = Guid.NewGuid().ToString() } };
        HttpTest.RespondWithJson(new { Docs = expected });

        var query = @"{""selector"":{""age"":19}}";
        var result = await Rebels.QueryAsync(query);
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_find")
            .WithVerb(HttpMethod.Post)
            .WithRequestBody(@"{""selector"":{""age"":19}}");
        Assert.Equal(expected.Count, result.Count);
        Assert.Equal(expected[0].Id, result[0].Id);
    }

    [Fact]
    public async Task QueryObject()
    {
        var expected = new List<Rebel>() { new Rebel { Id = Guid.NewGuid().ToString() } };
        HttpTest.RespondWithJson(new { Docs = expected });

        var query = new { selector = new { age = 19 } };
        var result = await Rebels.QueryAsync(query);
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_find")
            .WithVerb(HttpMethod.Post)
            .WithRequestBody(@"{""selector"":{""age"":19}}");
        Assert.Equal(expected.Count, result.Count);
        Assert.Equal(expected[0].Id, result[0].Id);
    }

    #endregion

    #region Bulk

    [Fact]
    public async Task ExecuteBulkItemOperations_Update()
    {
        // Response
        HttpTest.RespondWithJson(new[]
        {
            new { Id = "111", Ok = true, Rev = "111" },
            new { Id = "222", Ok = true, Rev = "222" },
        });
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        BulkItemOperation[] operations =
        [
            BulkItemOperation.Add(new Rebel { Name = "Luke", Id = "1" }),
            BulkItemOperation.Add(new Rebel { Name = "Leia", Id = "2" }),
        ];
        await Rebels.ExecuteBulkItemOperationsAsync(operations);
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_bulk_docs")
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task ExecuteBulkItemOperations_Delete()
    {
        // Response
        HttpTest.RespondWithJson(new[]
        {
            new { Id = "111", Ok = true, Rev = "111" },
            new { Id = "222", Ok = true, Rev = "222" },
        });
        // Logout
        HttpTest.RespondWithJson(new { ok = true });

        BulkItemOperation[] operations =
        [
            BulkItemOperation.Delete("1", "1"),
            BulkItemOperation.Delete("2", "1"),
        ];
        await Rebels.ExecuteBulkItemOperationsAsync(operations);
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_bulk_docs")
            .WithVerb(HttpMethod.Post);
    }

    #endregion

    #region View

    private static readonly string[] ExpectedViewKey = ["Luke", "Skywalker"];

    [Fact]
    public async Task QueryViewAsync_WithNoOptions_CallGet()
    {
        // Arrange
        SetupViewResponse();

        // Act
        var rebels = await Rebels.QueryViewAsync<string[], RebelView>("jedi", "by_name");

        // Assert
        Assert.Equal(10, rebels.Offset);
        Assert.Equal(20, rebels.TotalRows);
        var rebel = Assert.Single(rebels);
        Assert.Equal("luke", rebel.Id);
        Assert.Equal(ExpectedViewKey, rebel.Key);
        Assert.Equal(3, rebel.Value.NumberOfBattles);
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_design/jedi/_view/by_name")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task QueryViewAsync_WithOptions_CallPost()
    {
        // Arrange
        SetupViewResponse();
        var options = new CouchViewOptions<string[]>
        {
            Key = ["Luke", "Skywalker"],
            Skip = 10
        };

        // Act
        var rebels = await Rebels.QueryViewAsync<string[], RebelView>("jedi", "by_name", options);

        // Assert
        Assert.Equal(10, rebels.Offset);
        Assert.Equal(20, rebels.TotalRows);
        var rebel = Assert.Single(rebels);
        Assert.Equal("luke", rebel.Id);
        Assert.Equal(ExpectedViewKey, rebel.Key);
        Assert.Equal(3, rebel.Value.NumberOfBattles);
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_design/jedi/_view/by_name")
            .WithVerb(HttpMethod.Post)
            .WithRequestBody(@"{""key"":[""Luke"",""Skywalker""],""skip"":10}");
    }

    private void SetupViewResponse()
    {
        HttpTest.RespondWithJson(new
        {
            offset = 10,
            total_rows = 20,
            rows = new[]
            {
                new
                {
                    Id = "luke",
                    Key = ExpectedViewKey,
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
        SetupViewQueryResponse();
        var options = new CouchViewOptions<string[]>
        {
            Key = ["Luke", "Skywalker"],
            Skip = 10
        };
        var queries = new[]
        {
            options,
            options
        };

        // Act
        var results = await Rebels.QueryViewQueryAsync<string[], RebelView>("jedi", "by_name", queries);

        // Assert
        Assert.Equal(2, results.Length);

        Assert.All(results, result =>
        {
            Assert.Equal(10, result.Offset);
            Assert.Equal(20, result.TotalRows);
            var rebel = Assert.Single(result);
            Assert.Equal("luke", rebel.Id);
            Assert.Equal(ExpectedViewKey, rebel.Key);
            Assert.Equal(3, rebel.Value.NumberOfBattles);
        });
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_design/jedi/_view/by_name/queries")
            .WithVerb(HttpMethod.Post)
            .WithRequestBody(
                @"{""queries"":[{""key"":[""Luke"",""Skywalker""],""skip"":10},{""key"":[""Luke"",""Skywalker""],""skip"":10}]}");
    }

    private void SetupViewQueryResponse()
    {
        HttpTest.RespondWithJson(new
        {
            Results = new[]
            {
                new
                {
                    offset = 10,
                    total_rows = 20,
                    rows = new[]
                    {
                        new
                        {
                            Id = "luke",
                            Key = ExpectedViewKey,
                            Value = new
                            {
                                NumberOfBattles = 3
                            }
                        }
                    }
                },
                new
                {
                    offset = 10,
                    total_rows = 20,
                    rows = new[]
                    {
                        new
                        {
                            Id = "luke",
                            Key = ExpectedViewKey,
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
        HttpTest.RespondWithJson(new
        {
            cluster = new { },
            compact_running = false,
            db_name = "test_database",
            disk_format_version = 8,
            doc_count = 1000,
            doc_del_count = 50,
            purge_seq = "0-g1AAAABteJzLYWBg4MhgTmHgS04sKU7NS8",
            sizes = new { },
            update_seq = "1000-g1AAAABteJzLYWBg4MhgTmHgS04sKU7NS8",
            props = new { partitioned = false }
        });

        await Rebels.GetInfoAsync();
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task Compact()
    {
        // Operation response
        HttpTest.RespondWithJson(new { ok = true });

        await Rebels.CompactAsync();
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_compact")
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task SecurityInfo_Get()
    {
        HttpTest.RespondWithJson(new
        {
            admins = new
            {
                names = new[] { "superuser" },
                roles = new[] { "admins" }
            },
            members = new
            {
                names = new[] { "user1", "user2" },
                roles = new[] { "developers" }
            }
        });
        var securityInfo = await Rebels.Security.GetInfoAsync();
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_security")
            .WithVerb(HttpMethod.Get);
        Assert.Equal("user1", securityInfo.Members.Names[0]);
    }

    [Fact]
    public async Task SecurityInfo_Put()
    {
        HttpTest.RespondWithJson(new { ok = true });

        var securityInfo = new CouchSecurityInfo();
        securityInfo.Admins.Names.Add("user1");

        await Rebels.Security.SetInfoAsync(securityInfo);
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_security")
            .WithVerb(HttpMethod.Put)
            .WithRequestJson(securityInfo);
    }


    [Fact]
    public async Task GetRevLimit()
    {
        HttpTest.RespondWith("3");
        await Rebels.GetRevisionLimitAsync();
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_revs_limit")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task SetRevLimit()
    {
        // Operation response
        HttpTest.RespondWithJson(new { ok = true });

        await Rebels.SetRevisionLimitAsync(10);
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_revs_limit")
            .WithVerb(HttpMethod.Put);
    }

    #endregion
}