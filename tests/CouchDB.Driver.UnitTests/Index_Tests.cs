using System;
using System.Collections.Generic;
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
        public async Task CreateIndexAsync()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                result = "created"
            });

            await _rebels.CreateIndexAsync("skywalkers", b => b
                .IndexBy(r => r.Surname)
                .AlsoBy(r => r.Name)
                .Where(r => r.Surname == "Skywalker" && r.Age > 3)
                .OrderByDescending(r => r.Surname)
                .ThenByDescending(r => r.Name)
                .Take(100)
                .Skip(1),
                new IndexOptions()
                {
                    DesignDocument = "skywalkers_ddoc",
                    Partitioned = true
                });


            var expectedBody =
                "{\"index\":{\"selector\":{\"$and\":[{\"surname\":\"Skywalker\"},{\"age\":{\"$gt\":3}}]},\"fields\":[\"surname\",\"name\"],\"sort\":[{\"surname\":\"desc\"},{\"name\":\"desc\"}],\"limit\":100,\"skip\":1},\"name\":\"skywalkers\",\"type\":\"json\",\"ddoc\":\"skywalkers_ddoc\",\"partitioned\":true}";
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_index")
                .WithRequestBody(expectedBody)
                .WithVerb(HttpMethod.Post);
        }
    }
}
