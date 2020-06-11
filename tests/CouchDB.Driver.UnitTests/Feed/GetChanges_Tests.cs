using CouchDB.Driver.DTOs;
using CouchDB.Driver.UnitTests._Helpers;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Driver.UnitTests.Feed
{
    public class GetChanges_Tests
    {
        private readonly ICouchDatabase<Rebel> _rebels;

        public GetChanges_Tests()
        {
            var client = new CouchClient("http://localhost");
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
            var newR = await _rebels.GetChangesAsync();

            // Assert
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes")
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
            var newR = await _rebels.GetChangesAsync();

            // Assert
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes")
                .WithoutQueryParamValue("feed", "longpoll")
                .WithoutQueryParamValue("attachments", true)
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
            var filter = ChangesFeedFilter.DocumentIds(new[] 
            {
                docId
            });

            // Act
            var newR = await _rebels.GetChangesAsync(null, filter);

            // Assert
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes")
                .WithQueryParamValue("filter", "_doc_ids")
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
            var newR = await _rebels.GetChangesAsync(null, filter);

            // Assert
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes")
                .WithQueryParamValue("filter", "_selector")
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
            var newR = await _rebels.GetChangesAsync(null, filter);

            // Assert
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes")
                .WithQueryParamValue("filter", "_design")
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
            var newR = await _rebels.GetChangesAsync(null, filter);

            // Assert
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes")
                .WithQueryParamValue("filter", "_view")
                .WithQueryParamValue("view", view)
                .WithVerb(HttpMethod.Get);
        }

        private void SetFeedResponse(HttpTest httpTest)
        {
            httpTest.RespondWithJson(new ChangesFeedResponse<Rebel>
            {
                Results = new[]
                {
                    new ChangesFeedResponseResult<Rebel>
                    {
                        Id = "111",
                        Seq = "Seq111",
                        Changes = new[]
                        {
                            new ChangesFeedResponseResultChange
                            {
                                Rev = "111"
                            }
                        }
                    }
                }
            });
        }

    }
}
