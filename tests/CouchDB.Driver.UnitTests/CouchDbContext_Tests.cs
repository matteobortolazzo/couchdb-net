using System;
using System.Linq;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Options;
using CouchDB.UnitTests.Models;
using Flurl.Http.Testing;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class CouchDbContext_Tests
    {
        private class MyDeathStarContext: CouchContext
        {
            public CouchDatabase<Rebel> Rebels { get; set; }
            public CouchDatabase<OtherRebel> OtherRebels { get; set; }
            public CouchDatabase<SimpleRebel> SimpleRebels { get; set; }

            protected override void OnConfiguring(CouchOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseEndpoint("http://localhost:5984/")
                    .UseBasicAuthentication("admin", "admin");
            }

            protected override void OnDatabaseCreating(CouchDatabaseBuilder databaseBuilder)
            {
                databaseBuilder
                    .Document<OtherRebel>()
                    .ToDatabase("shared-rebels");

                databaseBuilder
                    .Document<SimpleRebel>()
                    .ToDatabase("shared-rebels");
            }
        }
        
        private class MyDeathStarContextCustomSplit: CouchContext
        {
            public CouchDatabase<OtherRebel> OtherRebels { get; set; }
            public CouchDatabase<SimpleRebel> SimpleRebels { get; set; }

            protected override void OnConfiguring(CouchOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseEndpoint("http://localhost:5984/")
                    .UseBasicAuthentication("admin", "admin")
                    .WithDatabaseSplitDiscriminator("docType");
            }

            protected override void OnDatabaseCreating(CouchDatabaseBuilder databaseBuilder)
            {
                databaseBuilder
                    .Document<OtherRebel>()
                    .ToDatabase("shared-rebels");

                databaseBuilder
                    .Document<SimpleRebel>()
                    .ToDatabase("shared-rebels");
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
        public async Task Context_Query_Discriminator()
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
                Id = "173694",
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
            await context.SimpleRebels.AddAsync(new SimpleRebel
            {
                Name = "Leia"
            });
            await context.OtherRebels.AddAsync(new OtherRebel
            {
                Name = "Luke"
            });
            var result = await context.OtherRebels.ToListAsync();
            Assert.NotEmpty(result);
            Assert.Equal("Luke", result[0].Name);
            Assert.Equal(@"{""name"":""Leia"",""age"":0,""split_discriminator"":""SimpleRebel"",""_attachments"":{}}", httpTest.CallLog[0].RequestBody);
            Assert.Equal(@"{""rebel_bith_date"":""0001-01-01T00:00:00"",""name"":""Luke"",""age"":0,""isJedi"":false,""species"":0,""guid"":""00000000-0000-0000-0000-000000000000"",""split_discriminator"":""OtherRebel"",""_attachments"":{}}", httpTest.CallLog[1].RequestBody);
            Assert.Equal(@"{""selector"":{""split_discriminator"":""OtherRebel""}}", httpTest.CallLog[2].RequestBody);
        }

        [Fact]
        public async Task Context_Query_Discriminator_Override()
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
                Id = "173694",
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

            await using var context = new MyDeathStarContextCustomSplit();
            await context.SimpleRebels.AddAsync(new SimpleRebel
            {
                Name = "Leia"
            });
            await context.OtherRebels.AddAsync(new OtherRebel
            {
                Name = "Luke"
            });
            var result = await context.OtherRebels.ToListAsync();
            Assert.NotEmpty(result);
            Assert.Equal("Luke", result[0].Name);
            Assert.Equal(@"{""name"":""Leia"",""age"":0,""docType"":""SimpleRebel"",""_attachments"":{}}", httpTest.CallLog[0].RequestBody);
            Assert.Equal(@"{""rebel_bith_date"":""0001-01-01T00:00:00"",""name"":""Luke"",""age"":0,""isJedi"":false,""species"":0,""guid"":""00000000-0000-0000-0000-000000000000"",""docType"":""OtherRebel"",""_attachments"":{}}", httpTest.CallLog[1].RequestBody);
            Assert.Equal(@"{""selector"":{""docType"":""OtherRebel""}}", httpTest.CallLog[2].RequestBody);
        }
        
        [Fact]
        public async Task Context_Query_MultiThread()
        {
            var context = new MyDeathStarContext();
            var tasks = new int[1000].Select(_ => Task.Run(() => context.Rebels.Take(int.MaxValue).ToString()));
            var results = await Task.WhenAll(tasks);
            
            Assert.All(results, query => Assert.Equal(@"{""limit"":2147483647,""selector"":{}}", query));
            await context.DisposeAsync();
        }
    }
}
