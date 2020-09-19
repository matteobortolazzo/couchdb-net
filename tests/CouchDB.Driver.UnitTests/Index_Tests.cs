using System.Net.Http;
using System.Threading.Tasks;
using CouchDB.Driver.Indexes;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Index_Tests
    {
        private readonly ICouchClient _client;
        private readonly ICouchDatabase<Rebel> _rebels;

        public Index_Tests()
        {
            _client = new CouchClient("http://localhost");
            _rebels = _client.GetDatabase<Rebel>();
        }

        [Fact]
        public async Task CreateIndex()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                result = "created"
            });

            await _rebels.CreateIndexAsync("skywalkers", b => b
                .IndexBy(r => r.Surname));


            var expectedBody =
                "{\"index\":{\"fields\":[\"surname\"]},\"name\":\"skywalkers\",\"type\":\"json\"}";
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_index")
                .WithRequestBody(expectedBody)
                .WithVerb(HttpMethod.Post);
        }

        [Fact]
        public async Task CreateIndex_WithOptions()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                result = "created"
            });

            await _rebels.CreateIndexAsync("skywalkers", b => b
                    .IndexByDescending(r => r.Surname)
                    .ThenByDescending(r => r.Name),
                new IndexOptions()
                {
                    DesignDocument = "skywalkers_ddoc",
                    Partitioned = true
                });


            var expectedBody =
                "{\"index\":{\"fields\":[{\"surname\":\"desc\"},{\"name\":\"desc\"}]},\"name\":\"skywalkers\",\"type\":\"json\",\"ddoc\":\"skywalkers_ddoc\",\"partitioned\":true}";
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_index")
                .WithRequestBody(expectedBody)
                .WithVerb(HttpMethod.Post);
        }

        [Fact]
        public async Task CreateIndex_Partial()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                result = "created"
            });

            await _rebels.CreateIndexAsync("skywalkers", b => b
                .IndexBy(r => r.Surname)
                .Where(r => r.Surname == "Skywalker"),
                new IndexOptions()
                {
                    DesignDocument = "skywalkers_ddoc",
                    Partitioned = true
                });


            var expectedBody =
                "{\"index\":{\"partial_filter_selector\":{\"surname\":\"Skywalker\"},\"fields\":[\"surname\"]},\"name\":\"skywalkers\",\"type\":\"json\",\"ddoc\":\"skywalkers_ddoc\",\"partitioned\":true}";
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_index")
                .WithRequestBody(expectedBody)
                .WithVerb(HttpMethod.Post);
        }
    }
}
