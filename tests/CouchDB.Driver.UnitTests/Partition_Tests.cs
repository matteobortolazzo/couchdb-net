using CouchDB.Driver.Types;
using CouchDB.UnitTests.Models;
using Flurl.Http.Testing;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Partition_Tests
    {
        private readonly ICouchClient _client;
        private readonly ICouchDatabase<Rebel> _rebels;

        public Partition_Tests()
        {
            _client = new CouchClient("http://localhost");
            _rebels = _client.GetDatabase<Rebel>();
        }

        [Fact]
        public async Task GetPartitionInfo()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                db_name = "rebels",
                doc_count = 10,
                doc_del_count = 2,
                partition = "skywalker",
                sizes = new
                {
                    active = 1024,
                    external = 2048
                }
            });

            var partitionInfo = await _rebels.GetPartitionInfoAsync("skywalker");
            
            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_partition/skywalker")
                .WithVerb(HttpMethod.Get);

            Assert.NotNull(partitionInfo);
            Assert.Equal("rebels", partitionInfo.DbName);
            Assert.Equal(10, partitionInfo.DocCount);
            Assert.Equal(2, partitionInfo.DocDelCount);
            Assert.Equal("skywalker", partitionInfo.Partition);
            Assert.NotNull(partitionInfo.Sizes);
        }

        [Fact]
        public async Task QueryPartitionWithJson()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                docs = new[]
                {
                    new { _id = "skywalker:luke", name = "Luke", surname = "Skywalker" },
                    new { _id = "skywalker:anakin", name = "Anakin", surname = "Skywalker" }
                },
                bookmark = "g1AAAAA",
                execution_stats = new { }
            });

            var query = "{\"selector\": {\"surname\": \"Skywalker\"}}";
            var results = await _rebels.QueryPartitionAsync("skywalker", query);

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_partition/skywalker/_find")
                .WithVerb(HttpMethod.Post)
                .WithContentType("application/json");

            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public async Task QueryPartitionWithObject()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                docs = new[]
                {
                    new { _id = "skywalker:luke", name = "Luke", surname = "Skywalker" }
                },
                bookmark = "g1AAAAA"
            });

            var query = new { selector = new { surname = "Skywalker" } };
            var results = await _rebels.QueryPartitionAsync("skywalker", query);

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_partition/skywalker/_find")
                .WithVerb(HttpMethod.Post);

            Assert.NotNull(results);
            Assert.Single(results);
        }

        [Fact]
        public async Task GetPartitionAllDocs()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                total_rows = 3,
                offset = 0,
                rows = new[]
                {
                    new 
                    { 
                        id = "skywalker:luke", 
                        key = "skywalker:luke",
                        value = new { rev = "1-abc" },
                        doc = new { _id = "skywalker:luke", _rev = "1-abc", name = "Luke", surname = "Skywalker" }
                    },
                    new 
                    { 
                        id = "skywalker:anakin", 
                        key = "skywalker:anakin",
                        value = new { rev = "1-def" },
                        doc = new { _id = "skywalker:anakin", _rev = "1-def", name = "Anakin", surname = "Skywalker" }
                    },
                    new 
                    { 
                        id = "skywalker:leia", 
                        key = "skywalker:leia",
                        value = new { rev = "1-ghi" },
                        doc = new { _id = "skywalker:leia", _rev = "1-ghi", name = "Leia", surname = "Skywalker" }
                    }
                }
            });

            var results = await _rebels.GetPartitionAllDocsAsync("skywalker");

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_partition/skywalker/_all_docs*")
                .WithQueryParam("include_docs", "true")
                .WithVerb(HttpMethod.Get);

            Assert.NotNull(results);
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public async Task GetPartitionInfo_WithSpecialCharacters()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                db_name = "rebels",
                doc_count = 5,
                doc_del_count = 0,
                partition = "user@email.com",
                sizes = new { active = 512, external = 1024 }
            });

            var partitionInfo = await _rebels.GetPartitionInfoAsync("user@email.com");

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_partition/user%40email.com")
                .WithVerb(HttpMethod.Get);

            Assert.NotNull(partitionInfo);
            Assert.Equal("user@email.com", partitionInfo.Partition);
        }

        [Fact]
        public async Task GetDatabaseInfo_WithPartitionedProperty()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                db_name = "rebels",
                doc_count = 100,
                doc_del_count = 10,
                cluster = new { },
                compact_running = false,
                disk_format_version = 8,
                purge_seq = "0",
                sizes = new { active = 10240, external = 20480 },
                update_seq = "100-g1AAAAA",
                props = new { partitioned = true }
            });

            var dbInfo = await _rebels.GetInfoAsync();

            httpTest
                .ShouldHaveCalled("http://localhost/rebels")
                .WithVerb(HttpMethod.Get);

            Assert.NotNull(dbInfo);
            Assert.NotNull(dbInfo.Props);
            Assert.True(dbInfo.Props.Partitioned);
        }
    }
}
