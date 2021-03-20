using CouchDB.UnitTests.Models;
using Flurl.Http.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Attachments_Tests: IAsyncDisposable
    {
        private readonly ICouchClient _client;
        private readonly ICouchDatabase<Rebel> _rebels;

        public Attachments_Tests()
        {
            _client = new CouchClient("http://localhost");
            _rebels = _client.GetDatabase<Rebel>();
        }

        [Fact]
        public void NewDocument_EmptyAttachmentsList()
        {
            var r = new Rebel { Name = "Luke" };
            Assert.Empty(r.Attachments);
        }

        [Fact]
        public void AddedAttachment_ShouldBeInList()
        {
            var r = new Rebel { Name = "Luke" };
            r.Attachments.AddOrUpdate("Assets/luke.txt", MediaTypeNames.Text.Plain);
            Assert.NotEmpty(r.Attachments);
        }

        [Fact]
        public void RemovedAttachment_ShouldBeNotInList()
        {
            var r = new Rebel { Name = "Luke" };
            r.Attachments.AddOrUpdate("Assets/luke.txt", MediaTypeNames.Text.Plain);
            r.Attachments.Delete("luke.txt");
            Assert.Empty(r.Attachments);
        }

        [Fact]
        public async Task DownloadAttachment()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                Id = "1",
                Ok = true,
                Rev = "xxx",
                Attachments = new Dictionary<string, object>
                    {
                        { "luke.txt", new { ContentType = "text/plain" } }
                    }
            });

            httpTest.RespondWithJson(new
            {
                Id = "1",
                Ok = true,
                Rev = "xxx2",
            });

            var r = new Rebel { Id = "1", Name = "Luke" };
            r.Attachments.AddOrUpdate("Assets/luke.txt", MediaTypeNames.Text.Plain);

            r = await _rebels.AddOrUpdateAsync(r);

            Types.CouchAttachment lukeTxt = r.Attachments.First();
            var newPath = await _rebels.DownloadAttachmentAsync(lukeTxt, "anyfolder");

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/1")
                .WithVerb(HttpMethod.Put);
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/1/luke.txt")
                .WithVerb(HttpMethod.Put)
                .WithHeader("If-Match", "xxx");
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/1/luke.txt")
                .WithVerb(HttpMethod.Get)
                .WithHeader("If-Match", "xxx2");

            Assert.Equal(@"anyfolder\luke.txt", newPath);
        }

        public ValueTask DisposeAsync()
        {
            return _client.DisposeAsync();
        }
    }
}
