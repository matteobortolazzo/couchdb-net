using System.Linq;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Options;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class CouchDbContext_Tests
    {
        private class MyDeathStarContext: CouchContext
        {
            public CouchDatabase<Rebel> Rebels { get; set; }

            protected override void OnConfiguring(CouchOptionsBuilder optionsBuilder)
            {
                optionsBuilder
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

            await using var context = new MyDeathStarContext();
            await context.Rebels.AddAsync(new Rebel
            {
                Name = "Luke"
            });
            var result = await context.Rebels.ToListAsync();
            Assert.NotEmpty(result);
            Assert.Equal("Luke", result[0].Name);
        }
        
        [Fact]
        public async Task Context_CreateIndex_Test()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                Id = "176694",
                Ok = true,
                Rev = "1-54f8e950cc338d2385d9b0cda2fd918e"
            });            

            await using var context = new MyDeathStarContext();
            
            await context.Rebels.IndexProvider.CreateAsync(index =>
            {
                index.IndexName = "test-index";
                index.Fields = x => new { x.Name, x.Age };
            });
            
            
            var call = httpTest.CallLog.First();
            Assert.NotNull(call);
            Assert.Equal(@"{""name"":""test-index"",""index"":{""fields"":[""Name"",""Age""]},""type"":""json""}", call.RequestBody);
        }        
    }
}
