using CouchDB.Driver.UnitTests._Helpers;
using CouchDB.UnitTests.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using CouchDB.Driver.ChangesFeed;
using CouchDB.Driver.ChangesFeed.Filters;
using CouchDB.Driver.ChangesFeed.Responses;
using Xunit;

namespace CouchDB.Driver.UnitTests.Feed;

public class GetChanges_Tests : HttpTests
{
    [Fact]
    public async Task GetChangesAsync_Default()
    {
        // Arrange
        SetFeedResponse();
        HttpTest.RespondWithJson(new { ok = true });

        // Act
        await Rebels.GetChangesAsync();

        // Assert
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task GetChangesAsync_WithOptions()
    {
        // Arrange
        SetFeedResponse();
        HttpTest.RespondWithJson(new { ok = true });
        var options = new ChangesFeedOptions
        {
            LongPoll = true,
            Attachments = true
        };

        // Act
        await Rebels.GetChangesAsync(options);

        // Assert
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes")
            .WithQueryParam("feed", "longpoll")
            .WithQueryParam("attachments", "true")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task GetChangesAsync_WithIdsFilter()
    {
        // Arrange
        SetFeedResponse();
        HttpTest.RespondWithJson(new { ok = true });

        var docId = Guid.NewGuid().ToString();
        var filter = ChangesFeedFilter.DocumentIds([
            docId
        ]);

        // Act
        await Rebels.GetChangesAsync(null, filter);

        // Assert
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes")
            .WithQueryParam("filter", "_doc_ids")
            .WithJsonBody<ChangesFeedFilterDocuments>(f => f.DocumentIds.Contains(docId))
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task GetChangesAsync_WithSelectorFilter()
    {
        // Arrange
        SetFeedResponse();
        HttpTest.RespondWithJson(new { ok = true });

        var docId = Guid.NewGuid().ToString();
        var filter = ChangesFeedFilter.Selector<Rebel>(rebel => rebel.Id == docId);

        // Act
        await Rebels.GetChangesAsync(null, filter);

        // Assert
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes")
            .WithQueryParam("filter", "_selector")
            .WithContentType("application/json")
            .With(call => call.RequestBody == $"{{\"selector\":{{\"_id\":\"{docId}\"}}}}")
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task GetChangesAsync_WithDesignFilter()
    {
        // Arrange
        SetFeedResponse();
        HttpTest.RespondWithJson(new { ok = true });

        var filter = ChangesFeedFilter.Design();

        // Act
        await Rebels.GetChangesAsync(null, filter);

        // Assert
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes")
            .WithQueryParam("filter", "_design")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task GetChangesAsync_WithViewFilter()
    {
        // Arrange
        SetFeedResponse();
        HttpTest.RespondWithJson(new { ok = true });

        var view = Guid.NewGuid().ToString();
        var filter = ChangesFeedFilter.View(view);

        // Act
        await Rebels.GetChangesAsync(null, filter);

        // Assert
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes")
            .WithQueryParam("filter", "_view")
            .WithQueryParam("view", view)
            .WithVerb(HttpMethod.Get);
    }

    private void SetFeedResponse()
    {
        ChangesFeedResponseResultChange[] changes =
        [
            new()
            {
                Rev = "111"
            }
        ];
        HttpTest.RespondWithJson(new ChangesFeedResponse<Rebel>
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