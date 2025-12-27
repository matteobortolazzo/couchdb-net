using CouchDB.Driver.UnitTests._Helpers;
using CouchDB.UnitTests.Models;
using Flurl.Http.Testing;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using CouchDB.Driver.ChangesFeed;
using CouchDB.Driver.ChangesFeed.Filters;
using CouchDB.Driver.ChangesFeed.Responses;
using Xunit;

namespace CouchDB.Driver.UnitTests.Feed;

public class GetChanges_Tests
{
    private readonly ICouchDatabase<Rebel> _rebels;

    public GetChanges_Tests()
    {
        var client = new CouchClient("http://localhost", new BasicCredentials("admin", "admin"));
        _rebels = client.GetDatabase<Rebel>();
    }

    [Fact]
    public async Task GetChangesAsync_Default()
    {
        using var httpTest = new HttpTest();

        // Arrange
        SetFeedResponse(httpTest);
        httpTest.RespondWithJson(new { ok = true });

        // Act
        await _rebels.GetChangesAsync();

        // Assert
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes*")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task GetChangesAsync_WithOptions()
    {
        using var httpTest = new HttpTest();

        // Arrange
        SetFeedResponse(httpTest);
        httpTest.RespondWithJson(new { ok = true });
        var options = new ChangesFeedOptions
        {
            LongPoll = true,
            Attachments = true
        };

        // Act
        await _rebels.GetChangesAsync(options);

        // Assert
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes*")
            .WithQueryParam("feed", "longpoll")
            .WithQueryParam("attachments", "true")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task GetChangesAsync_WithIdsFilter()
    {
        using var httpTest = new HttpTest();

        // Arrange
        SetFeedResponse(httpTest);
        httpTest.RespondWithJson(new { ok = true });

        var docId = Guid.NewGuid().ToString();
        var filter = ChangesFeedFilter.DocumentIds([
            docId
        ]);

        // Act
        await _rebels.GetChangesAsync(null, filter);

        // Assert
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes**")
            .WithQueryParam("filter", "_doc_ids")
            .WithJsonBody<ChangesFeedFilterDocuments>(f => f.DocumentIds.Contains(docId))
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task GetChangesAsync_WithSelectorFilter()
    {
        using var httpTest = new HttpTest();

        // Arrange
        SetFeedResponse(httpTest);
        httpTest.RespondWithJson(new { ok = true });

        var docId = Guid.NewGuid().ToString();
        var filter = ChangesFeedFilter.Selector<Rebel>(rebel => rebel.Id == docId);

        // Act
        await _rebels.GetChangesAsync(null, filter);

        // Assert
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes*")
            .WithQueryParam("filter", "_selector")
            .WithContentType("application/json")
            .With(call => call.RequestBody == $"{{\"selector\":{{\"_id\":\"{docId}\"}}}}")
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task GetChangesAsync_WithDesignFilter()
    {
        using var httpTest = new HttpTest();

        // Arrange
        SetFeedResponse(httpTest);
        httpTest.RespondWithJson(new { ok = true });

        var filter = ChangesFeedFilter.Design();

        // Act
        await _rebels.GetChangesAsync(null, filter);

        // Assert
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes*")
            .WithQueryParam("filter", "_design")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task GetChangesAsync_WithViewFilter()
    {
        using var httpTest = new HttpTest();

        // Arrange
        SetFeedResponse(httpTest);
        httpTest.RespondWithJson(new { ok = true });

        var view = Guid.NewGuid().ToString();
        var filter = ChangesFeedFilter.View(view);

        // Act
        await _rebels.GetChangesAsync(null, filter);

        // Assert
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes*")
            .WithQueryParam("filter", "_view")
            .WithQueryParam("view", view)
            .WithVerb(HttpMethod.Get);
    }

    private static void SetFeedResponse(HttpTest httpTest)
    {
        ChangesFeedResponseResultChange[] changes =
        [
            new()
            {
                Rev = "111"
            }
        ];
        httpTest.RespondWithJson(new ChangesFeedResponse<Rebel>
        {
            LastSequence = "",
            Pending = 0,
            Results =
            [
                new ChangesFeedResponseResult<Rebel>(
                    "111",
                    "Seq111",
                    false,
                    changes,
                    [],
                    DateTime.Now,
                    "",
                    new Rebel())
            ]
        });
    }
}