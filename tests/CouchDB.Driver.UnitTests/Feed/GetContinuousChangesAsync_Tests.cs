using CouchDB.Driver.UnitTests._Helpers;
using CouchDB.UnitTests.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.ChangesFeed;
using CouchDB.Driver.ChangesFeed.Filters;
using CouchDB.Driver.ChangesFeed.Responses;
using Xunit;

namespace CouchDB.Driver.UnitTests.Feed;

public class GetContinuousChangesAsync_Tests : HttpTests
{
    [Fact]
    public async Task GetContinuousChangesAsync_Default()
    {
        // Arrange
        var tokenSource = new CancellationTokenSource();
        var docId = SetFeedResponse();
        HttpTest.RespondWithJson(new { ok = true });

        // Act
        await foreach (var change in Rebels.GetContinuousChangesAsync(null, null, tokenSource.Token))
        {
            Assert.Equal(docId, change.Id);
            await tokenSource.CancelAsync();
        }

        // Assert
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes")
            .WithQueryParam("feed", "continuous")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task GetContinuousChangesAsync_WithOptions()
    {
        // Arrange
        var tokenSource = new CancellationTokenSource();
        var docId = SetFeedResponse();
        HttpTest.RespondWithJson(new { ok = true });
        var options = new ChangesFeedOptions
        {
            Attachments = true
        };

        // Act
        await foreach (var change in Rebels.GetContinuousChangesAsync(options, null, tokenSource.Token))
        {
            Assert.Equal(docId, change.Id);
            await tokenSource.CancelAsync();
        }

        // Assert
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes")
            .WithQueryParam("feed", "continuous")
            .WithQueryParam("attachments", "true")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task GetContinuousChangesAsync_WithIdsFilter()
    {
        // Arrange
        var tokenSource = new CancellationTokenSource();
        var docId = SetFeedResponse();
        HttpTest.RespondWithJson(new { ok = true });


        var filter = ChangesFeedFilter.DocumentIds([
            docId
        ]);

        // Act
        await foreach (var change in Rebels.GetContinuousChangesAsync(null, filter, tokenSource.Token))
        {
            Assert.Equal(docId, change.Id);
            await tokenSource.CancelAsync();
        }

        // Assert
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes")
            .WithQueryParam("feed", "continuous")
            .WithQueryParam("filter", "_doc_ids")
            .WithJsonBody<ChangesFeedFilterDocuments>(f => f.DocumentIds.Contains(docId))
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task GetContinuousChangesAsync_WithSelectorFilter()
    {
        // Arrange
        var tokenSource = new CancellationTokenSource();
        var docId = SetFeedResponse();
        HttpTest.RespondWithJson(new { ok = true });

        var filter = ChangesFeedFilter.Selector<Rebel>(rebel => rebel.Id == docId);

        // Act
        await foreach (var change in Rebels.GetContinuousChangesAsync(null, filter, tokenSource.Token))
        {
            Assert.Equal(docId, change.Id);
            await tokenSource.CancelAsync();
        }

        // Assert
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes")
            .WithQueryParam("feed", "continuous")
            .WithQueryParam("filter", "_selector")
            .WithContentType("application/json")
            .With(call => call.RequestBody == $"{{\"selector\":{{\"_id\":\"{docId}\"}}}}")
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task GetContinuousChangesAsync_WithDesignFilter()
    {
        // Arrange
        var tokenSource = new CancellationTokenSource();
        var docId = SetFeedResponse();
        HttpTest.RespondWithJson(new { ok = true });

        var filter = ChangesFeedFilter.Design();

        // Act
        await foreach (var change in Rebels.GetContinuousChangesAsync(null, filter, tokenSource.Token))
        {
            Assert.Equal(docId, change.Id);
            await tokenSource.CancelAsync();
        }

        // Assert
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes")
            .WithQueryParam("feed", "continuous")
            .WithQueryParam("filter", "_design")
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task GetContinuousChangesAsync_WithViewFilter()
    {
        // Arrange
        var tokenSource = new CancellationTokenSource();
        var docId = SetFeedResponse();
        HttpTest.RespondWithJson(new { ok = true });

        var view = Guid.NewGuid().ToString();
        var filter = ChangesFeedFilter.View(view);

        // Act
        await foreach (var change in Rebels.GetContinuousChangesAsync(null, filter, tokenSource.Token))
        {
            Assert.Equal(docId, change.Id);
            await tokenSource.CancelAsync();
        }

        // Assert
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes")
            .WithQueryParam("feed", "continuous")
            .WithQueryParam("filter", "_view")
            .WithQueryParam("view", view)
            .WithVerb(HttpMethod.Get);
    }

    [Fact]
    public async Task GetContinuousChangesAsync_MultiResponse()
    {
        // Arrange
        var tokenSource = new CancellationTokenSource();
        var docId = Guid.NewGuid().ToString();
        var changeJson = GetChangesFeedResponseResultJson(docId);
        HttpTest.RespondWith(changeJson + changeJson + changeJson + "\n");
        HttpTest.RespondWithJson(new { ok = true });

        // Act
        var changesCount = 0;
        await foreach (var change in Rebels.GetContinuousChangesAsync(null, null, tokenSource.Token))
        {
            changesCount++;
            Assert.Equal(docId, change.Id);
            await tokenSource.CancelAsync();
        }

        // Assert
        Assert.Equal(3, changesCount);
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_changes")
            .WithQueryParam("feed", "continuous")
            .WithVerb(HttpMethod.Get);
    }

    private string SetFeedResponse()
    {
        var docId = Guid.NewGuid().ToString();
        var changeJson = GetChangesFeedResponseResultJson(docId);

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(changeJson + "\n"));
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentLength = stream.Length;

        HttpTest.RespondWith(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = streamContent
        });

        return docId;
    }

    private static string GetChangesFeedResponseResultJson(string docId)
    {
        ChangesFeedResponseResultChange[] changes =
        [
            new()
            {
                Rev = $"{Guid.NewGuid():N}"
            }
        ];
        return JsonSerializer.Serialize(new ChangesFeedResponseResult<Rebel>(
            $"{Guid.NewGuid():N}",
            docId,
            false,
            changes,
            [],
            DateTime.Now,
            "",
            new Rebel())
        );
    }
}