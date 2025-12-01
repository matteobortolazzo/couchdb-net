using CouchDB.Driver.ChangesFeed;
using CouchDB.UnitTests.Models;
using Flurl.Http.Testing;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class ChangesFeed_DesignDocumentFilter_Tests
    {
        private readonly ICouchClient _client;
        private readonly ICouchDatabase<Rebel> _rebels;

        public ChangesFeed_DesignDocumentFilter_Tests()
        {
            _client = new CouchClient("http://localhost");
            _rebels = _client.GetDatabase<Rebel>();
        }

        [Fact]
        public async Task GetChangesAsync_WithDesignDocumentFilter_AppliesFilterName()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                results = new object[] { },
                last_seq = "123-abc"
            });

            var filter = ChangesFeedFilter.DesignDocument("replication/by_partition");
            await _rebels.GetChangesAsync(filter: filter);

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes*")
                .WithQueryParam("filter", "replication/by_partition")
                .WithVerb(HttpMethod.Get);
        }

        [Fact]
        public async Task GetChangesAsync_WithDesignDocumentFilterAndQueryParams_AppliesParameters()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                results = new object[] { },
                last_seq = "123-abc"
            });

            var queryParams = new Dictionary<string, string>
            {
                { "partition", "skywalker" },
                { "status", "active" }
            };
            var filter = ChangesFeedFilter.DesignDocument("replication/by_partition", queryParams);
            await _rebels.GetChangesAsync(filter: filter);

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes*")
                .WithQueryParam("filter", "replication/by_partition")
                .WithQueryParam("partition", "skywalker")
                .WithQueryParam("status", "active")
                .WithVerb(HttpMethod.Get);
        }

        [Fact]
        public async Task GetChangesAsync_WithOptionsQueryParameters_AppliesParameters()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                results = new object[] { },
                last_seq = "123-abc"
            });

            var options = new ChangesFeedOptions
            {
                Filter = "replication/by_partition",
                QueryParameters = new Dictionary<string, string>
                {
                    { "partition", "skywalker" }
                }
            };

            await _rebels.GetChangesAsync(options: options);

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_changes*")
                .WithQueryParam("filter", "replication/by_partition")
                .WithQueryParam("partition", "skywalker")
                .WithVerb(HttpMethod.Get);
        }
    }
}
