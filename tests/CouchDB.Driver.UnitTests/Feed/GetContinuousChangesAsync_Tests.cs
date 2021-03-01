using CouchDB.Driver.DTOs;
using CouchDB.Driver.UnitTests._Helpers;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.ChangesFeed;
using CouchDB.Driver.ChangesFeed.Filters;
using CouchDB.Driver.ChangesFeed.Responses;
using Xunit;

namespace CouchDB.Driver.UnitTests.Feed
{
    public class GetContinuousChangesAsync_Tests
    {
        private readonly ICouchDatabase<Rebel> _rebels;

        public GetContinuousChangesAsync_Tests()
        {
            var client = new CouchClient("http://localhost");
            _rebels = client.GetDatabase<Rebel>();
        }

        [Fact]
        public async Task GetContinuousChangesAsync_Default()
        {
            using var httpTest = new HttpTest();

            // Arrange
            var tokenSource = new CancellationTokenSource();
            var docId= SetFeedResponse(httpTest);
            httpTest.RespondWithJson(new { ok = true });

            // Act
            await foreach(var change in _rebels.GetContinuousChangesAsync(null, null, tokenSource.Token))
            {
                Assert.Equal(docId, change.Id);
                tokenSource.Cancel();
            }

            // Assert
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes*")
                .WithQueryParam("feed", "continuous")
                .WithVerb(HttpMethod.Get);
        }

        [Fact]
        public async Task GetContinuousChangesAsync_WithOptions()
        {
            using var httpTest = new HttpTest();

            // Arrange
            var tokenSource = new CancellationTokenSource();
            var docId = SetFeedResponse(httpTest);
            httpTest.RespondWithJson(new { ok = true });
            var options = new ChangesFeedOptions
            {
                Attachments = true
            };

            // Act
            await foreach (var change in _rebels.GetContinuousChangesAsync(options, null, tokenSource.Token))
            {
                Assert.Equal(docId, change.Id);
                tokenSource.Cancel();
            }

            // Assert
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes*")
                .WithQueryParam("feed", "continuous")
                .WithQueryParam("attachments", "true")
                .WithVerb(HttpMethod.Get);
        }

        [Fact]
        public async Task GetContinuousChangesAsync_WithIdsFilter()
        {
            using var httpTest = new HttpTest();

            // Arrange
            var tokenSource = new CancellationTokenSource();
            var docId = SetFeedResponse(httpTest);
            httpTest.RespondWithJson(new { ok = true });

            var filter = ChangesFeedFilter.DocumentIds(new[] 
            {
                docId
            });

            // Act
            await foreach (var change in _rebels.GetContinuousChangesAsync(null, filter, tokenSource.Token))
            {
                Assert.Equal(docId, change.Id);
                tokenSource.Cancel();
            }

            // Assert
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes*")
                .WithQueryParam("feed", "continuous")
                .WithQueryParam("filter", "_doc_ids")
                .WithJsonBody<ChangesFeedFilterDocuments>(f => f.DocumentIds.Contains(docId))
                .WithVerb(HttpMethod.Post);
        }

        [Fact]
        public async Task GetContinuousChangesAsync_WithSelectorFilter()
        {
            using var httpTest = new HttpTest();

            // Arrange
            var tokenSource = new CancellationTokenSource();
            var docId = SetFeedResponse(httpTest);
            httpTest.RespondWithJson(new { ok = true });

            var filter = ChangesFeedFilter.Selector<Rebel>(rebel => rebel.Id == docId);

            // Act
            await foreach (var change in _rebels.GetContinuousChangesAsync(null, filter, tokenSource.Token))
            {
                Assert.Equal(docId, change.Id);
                tokenSource.Cancel();
            }

            // Assert
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes*")
                .WithQueryParam("feed", "continuous")
                .WithQueryParam("filter", "_selector")
                .WithContentType("application/json")
                .With(call => call.RequestBody == $"{{\"selector\":{{\"_id\":\"{docId}\"}}}}")
                .WithVerb(HttpMethod.Post);
        }

        [Fact]
        public async Task GetContinuousChangesAsync_WithDesignFilter()
        {
            using var httpTest = new HttpTest();

            // Arrange
            var tokenSource = new CancellationTokenSource();
            var docId = SetFeedResponse(httpTest);
            httpTest.RespondWithJson(new { ok = true });

            var filter = ChangesFeedFilter.Design();

            // Act
            await foreach (var change in _rebels.GetContinuousChangesAsync(null, filter, tokenSource.Token))
            {
                Assert.Equal(docId, change.Id);
                tokenSource.Cancel();
            }

            // Assert
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes*")
                .WithQueryParam("feed", "continuous")
                .WithQueryParam("filter", "_design")
                .WithVerb(HttpMethod.Get);
        }

        [Fact]
        public async Task GetContinuousChangesAsync_WithViewFilter()
        {
            using var httpTest = new HttpTest();

            // Arrange
            var tokenSource = new CancellationTokenSource();
            var docId = SetFeedResponse(httpTest);
            httpTest.RespondWithJson(new { ok = true });

            var view = Guid.NewGuid().ToString();
            var filter = ChangesFeedFilter.View(view);

            // Act
            await foreach (var change in _rebels.GetContinuousChangesAsync(null, filter, tokenSource.Token))
            {
                Assert.Equal(docId, change.Id);
                tokenSource.Cancel();
            }

            // Assert
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes*")
                .WithQueryParam("feed", "continuous")
                .WithQueryParam("filter", "_view")
                .WithQueryParam("view", view)
                .WithVerb(HttpMethod.Get);
        }

        private string SetFeedResponse(HttpTest httpTest)
        {
            var docId = Guid.NewGuid().ToString();
            var changeJson = JsonConvert.SerializeObject(new ChangesFeedResponseResult<Rebel>
            {
                Id = docId
            });
            changeJson += "\n";
            httpTest.RespondWith(changeJson);
            return docId;
        }
    }
}
