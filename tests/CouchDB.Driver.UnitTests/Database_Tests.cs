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

public class Database_Tests : HttpTest
{
    #region Crud

    [Fact]
    public async Task Find()
    {
        httpTest.RespondWithJson(new
        {
            _attachments = new Dictionary<string, object>
            {
                { "luke.txt", new { ContentType = "text/plain" } }
            }
        });

        var newR = await _rebels.ReadItemAsync("1");
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/1")
            .WithoutQueryParam("conflicts")
            .WithVerb(HttpMethod.Get);

        Assert.NotNull(newR);
        Assert.NotNull(newR.Attachments);
        Assert.NotEmpty(newR.Attachments);
    }

    [Fact]
    public async Task FindWithConflicts()
    {
        httpTest.RespondWithJson(new
        {
            _attachments = new Dictionary<string, object>
            {
                { "luke.txt", new { ContentType = "text/plain" } }
            }
        });

        await _rebels.ReadItemAsync("1", new ReadItemOptions
        {
            Conflicts = true
        });
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/1*")
            .WithQueryParam("conflicts", "true")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task FindWithOptionsRevision()
    {
        httpTest.RespondWithJson(new
        {
            _attachments = new Dictionary<string, object>
            {
                { "luke.txt", new { ContentType = "text/plain" } }
            }
        });

        await _rebels.ReadItemAsync("1", new ReadItemOptions { Revision = "1-xxx" });
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/1*")
            .WithQueryParam("rev", "1-xxx")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task FindWithOptionsConflicts()
    {
        httpTest.RespondWithJson(new
        {
            _attachments = new Dictionary<string, object>
            {
                { "luke.txt", new { ContentType = "text/plain" } }
            }
        });

        await _rebels.ReadItemAsync("1", new ReadItemOptions { Conflicts = true });
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/1*")
            .WithQueryParam("conflicts", "true")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task FindMany()
    {
        httpTest.RespondWith(
            @"{""results"":[{""id"":""1"",""docs"":[{""ok"":{""_id"":""1"",""Name"":""Luke""}}]},{""id"":""2"",""docs"":[{""ok"":{""_id"":""2"",""Name"":""Leia""}}]}]}");
        var ids = new[] { "1", "2" };
        var result = await _rebels.ReadItemsAsync(ids);
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
        httpTest.RespondWith(
            @"{""results"":[{""id"":""1"",""docs"":[{""error"":{""id"":""1"",""rev"":""undefined"",""error"":""not_found"",""reason"":""missing""}}]}]}");
        var ids = new[] { "1" };
        var result = await _rebels.ReadItemsAsync(ids);
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
        httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "xxx" });

        var r = new Rebel { Name = "Luke" };
        await _rebels.CreateItemAsync(r);
        httpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task CreateWithOptionsBatch()
    {
        httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "xxx" });

        var r = new Rebel { Name = "Luke" };
        await _rebels.CreateItemAsync(r, new CreateItemRequestOptions
        {
            Batch = true
        });
        httpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithQueryParam("batch", "ok")
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task CreateWithOptionsRevision()
    {
        httpTest.RespondWithJson(new { Id = "xxx", Ok = true, Rev = "2-xxx" });

        var r = new Rebel { Name = "Luke", Id = "1" };
        await _rebels.UpdateItemAsync(r, r.Id, "1-xxx");
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/1")
            .WithQueryParam("rev", "1-xxx")
            .WithVerb(HttpMethod.Put);
    }

    [Fact]
    public async Task Create_WithoutId()
    {
        var exception = await Record.ExceptionAsync(async () =>
        {
            var r = new Rebel { Name = "Luke" };
            await _rebels.CreateItemAsync(r);
        });
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task Delete()
    {
        // Operation response
        httpTest.RespondWithJson(new { ok = true });

        await _rebels.DeleteItemAsync("Id", "1");
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/1?rev=1")
            .WithVerb(HttpMethod.Delete);
    }

    [Fact]
    public async Task CouchList()
    {
        // ToList
        httpTest.RespondWithJson(new { Docs = new List<string>(), Bookmark = "bookmark" });
        // Operation response
        httpTest.RespondWithJson(new { ok = true });

        await using var client = TestCouchClientFactory.Create(httpTest);
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
        // Response
        httpTest.RespondWithJson(new[]
        {
            new { Id = "111", Ok = true, Rev = "111" },
            new { Id = "222", Ok = true, Rev = "222" },
        });
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        BulkItemOperation[] operations =
        [
            BulkItemOperation.Add(new Rebel { Name = "Luke", Id = "1" }),
            BulkItemOperation.Add(new Rebel { Name = "Leia", Id = "2" }),
        ];
        await _rebels.ExecuteBulkItemOperationsAsync(operations);
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_bulk_docs")
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task DeleteRange()
    {
        // Response
        httpTest.RespondWithJson(new[]
        {
            new { Id = "111", Ok = true, Rev = "111" },
            new { Id = "222", Ok = true, Rev = "222" },
        });
        // Logout
        httpTest.RespondWithJson(new { ok = true });

        BulkItemOperation[] operations =
        [
            BulkItemOperation.Delete("1", "1"),
            BulkItemOperation.Delete("2", "1"),
        ];
        await _rebels.ExecuteBulkItemOperationsAsync(operations);
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_bulk_docs")
            .WithVerb(HttpMethod.Post);
    }

    #endregion

    #region View

    private static readonly string[] ExpectedViewKey = ["Luke", "Skywalker"];

    [Fact]
    public async Task GetViewAsync_WithNoOptions_CallGet()
    {
        // Arrange
        SetupViewResponse();

        // Act
        var rebels = await _rebels.GetViewAsync<string[], RebelView>("jedi", "by_name");

        // Assert
        var rebel = Assert.Single(rebels);
        Assert.Equal("luke", rebel.Id);
        Assert.Equal(ExpectedViewKey, rebel.Key);
        Assert.Equal(3, rebel.Value.NumberOfBattles);
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_design/jedi/_view/by_name")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task GetViewAsync_WithOptions_CallPost()
    {
        // Arrange
        SetupViewResponse();
        var options = new CouchViewOptions<string[]>
        {
            Key = ["Luke", "Skywalker"],
            Skip = 10
        };

        // Act
        var rebels = await _rebels.GetViewAsync<string[], RebelView>("jedi", "by_name", options);

        // Assert
        var rebel = Assert.Single(rebels);
        Assert.Equal("luke", rebel.Id);
        Assert.Equal(ExpectedViewKey, rebel.Key);
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
        SetupViewResponse();

        // Act
        var list = await _rebels.GetDetailedViewAsync<string[], RebelView>("jedi", "by_name");

        // Assert
        Assert.Equal(10, list.Offset);
        Assert.Equal(20, list.TotalRows);
        var rebel = Assert.Single(list.Rows);
        Assert.Equal("luke", rebel.Id);
        Assert.Equal(ExpectedViewKey, rebel.Key);
        Assert.Equal(3, rebel.Value.NumberOfBattles);
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_design/jedi/_view/by_name")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task GetDetailedViewAsync_WithOptions_CallPost()
    {
        // Arrange
        SetupViewResponse();
        var options = new CouchViewOptions<string[]>
        {
            Key = ["Luke", "Skywalker"],
            Update = UpdateStyle.Lazy
        };

        // Act
        var list = await _rebels.GetDetailedViewAsync<string[], RebelView>("jedi", "by_name", options);

        // Assert
        Assert.Equal(10, list.Offset);
        Assert.Equal(20, list.TotalRows);
        var rebel = Assert.Single(list.Rows);
        Assert.Equal("luke", rebel.Id);
        Assert.Equal(ExpectedViewKey, rebel.Key);
        Assert.Equal(3, rebel.Value.NumberOfBattles);
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_design/jedi/_view/by_name")
            .WithVerb(HttpMethod.Post)
            .WithRequestBody(@"{""key"":[""Luke"",""Skywalker""],""update"":""lazy""}");
    }

    private void SetupViewResponse()
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
        var results = await _rebels.GetViewQueryAsync<string[], RebelView>("jedi", "by_name", queries);

        // Assert
        Assert.Equal(2, results.Length);

        Assert.All(results, result =>
        {
            var rebel = Assert.Single(result);
            Assert.Equal("luke", rebel.Id);
            Assert.Equal(ExpectedViewKey, rebel.Key);
            Assert.Equal(3, rebel.Value.NumberOfBattles);
        });
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_design/jedi/_view/by_name/queries")
            .WithVerb(HttpMethod.Post)
            .WithRequestBody(
                @"{""queries"":[{""key"":[""Luke"",""Skywalker""],""skip"":10},{""key"":[""Luke"",""Skywalker""],""skip"":10}]}");
    }

    [Fact]
    public async Task GetDetailedViewQueryAsync()
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
        var results = await _rebels.GetDetailedViewQueryAsync<string[], RebelView>("jedi", "by_name", queries);

        // Assert
        Assert.Equal(2, results.Length);

        Assert.All(results, result =>
        {
            Assert.Equal(10, result.Offset);
            Assert.Equal(20, result.TotalRows);
            var rebel = Assert.Single(result.Rows);
            Assert.Equal("luke", rebel.Id);
            Assert.Equal(ExpectedViewKey, rebel.Key);
            Assert.Equal(3, rebel.Value.NumberOfBattles);
        });
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_design/jedi/_view/by_name/queries")
            .WithVerb(HttpMethod.Post)
            .WithRequestBody(
                @"{""queries"":[{""key"":[""Luke"",""Skywalker""],""skip"":10},{""key"":[""Luke"",""Skywalker""],""skip"":10}]}");
    }

    private void SetupViewQueryResponse()
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
                    Offset = 10,
                    Total_Rows = 20,
                    Rows = new[]
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
        await _rebels.GetInfoAsync();
        httpTest
            .ShouldHaveCalled("http://localhost/rebels")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task Compact()
    {
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
        httpTest.RespondWithJson(new { ok = true });

        var securityInfo = new CouchSecurityInfo();
        securityInfo.Admins.Names.Add("user1");

        await _rebels.Security.SetInfoAsync(securityInfo);
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_security")
            .WithVerb(HttpMethod.Put)
            .WithRequestJson(securityInfo);
    }


    [Fact]
    public async Task GetRevLimit()
    {
        httpTest.RespondWith("3");
        await _rebels.GetRevisionLimitAsync();
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_revs_limit")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task SetRevLimit()
    {
        // Operation response
        httpTest.RespondWithJson(new { ok = true });

        await _rebels.SetRevisionLimitAsync(10);
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_revs_limit")
            .WithVerb(HttpMethod.Put);
    }

    #endregion
}