using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Settings;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class CouchDbContext_Tests
    {
        private class TestContext: CouchDbContext
        {
            public CouchDatabase<Rebel> Rebels { get; set; }

            protected override void OnConfiguring(ICouchContextConfigurator configurator)
            {
                configurator
                    .UseEndpoint("http://localhost:5984/")
                    .UseBasicAuthentication("admin", "admin");
            }
        }

        [Fact]
        public async Task Context_Query()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                Id = "176694",
                Ok = true,
                Rev = "1-54f8e950cc338d2385d9b0cda2fd918e"
            });
            httpTest.RespondWithJson(new
            {
                docs = new object[] {
                    new {
                        Id = "176694",
                        Rev = "1-54f8e950cc338d2385d9b0cda2fd918e",
                        Name = "Luke"
                    }
                }
            });

            await using var context = new TestContext();
            await context.Rebels.CreateAsync(new Rebel
            {
                Name = "Luke"
            });
            var result = await context.Rebels.ToListAsync();
            Assert.NotEmpty(result);
            Assert.Equal("Luke", result[0].Name);
        }
    }
}
