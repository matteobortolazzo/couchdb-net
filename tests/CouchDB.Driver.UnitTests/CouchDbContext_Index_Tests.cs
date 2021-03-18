using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Options;
using CouchDB.UnitTests.Models;
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
                    .OverrideExistingIndexes()
                    .UseBasicAuthentication("admin", "admin");
            }

            protected override void OnDatabaseCreating(CouchDatabaseBuilder databaseBuilder)
            {
                databaseBuilder.Document<Rebel>()
                    .HasIndex("skywalkers", b => b.IndexBy(b => b.Surname));
            }
        }
        
        [Fact]
        public async Task Context_Index()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                Indexes = new string[0]
            });
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

        [Fact]
        public async Task Context_Index_NotToOverride()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                Indexes = new []
                {
                    new {
                        ddoc = Guid.NewGuid().ToString(),
                        name = "skywalkers",
                        def = new {
                            fields = new[] {
                                new Dictionary<string, string>{ { "surname", "asc" } }
                            }
                        }
                    }
                }
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

        [Fact]
        public async Task Context_Index_ToOverride()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                Indexes = new[]
                {
                    new {
                        ddoc = Guid.NewGuid().ToString(),
                        name = "skywalkers",
                        def = new {
                            fields = new[] {
                                new Dictionary<string, string>{ { "surname", "desc" } }
                            }
                        }
                    }
                }
            });
            // Delete
            httpTest.RespondWithJson(new
            {
                ok = true
            });
            // Create new 
            httpTest.RespondWithJson(new
            {
                Result = "created"
            });
            // Query
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
