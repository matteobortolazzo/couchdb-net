using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Options;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class CouchDbContext_Index_Tests
    {
        private class MyDeathStarContext : CouchContext
        {
            public CouchDatabase<Rebel> Rebels { get; set; }

            protected override void OnConfiguring(CouchOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseEndpoint("http://localhost:5984/")
                    .UseBasicAuthentication("admin", "admin");
            }

            protected override void OnDatabaseCreating(CouchDatabaseBuilder databaseBuilder)
            {
                databaseBuilder.Document<Rebel>()
                    .HasIndex("skywalkers", b => b.IndexBy(b => b.Surname));
            }
        }
        
        [Fact]
        public async Task Context_Query()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                Result = "created"
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

            await using var context = new MyDeathStarContext();
            var result = await context.Rebels.ToListAsync();
            Assert.NotEmpty(result);
            Assert.Equal("Luke", result[0].Name);
        }
    }
}
