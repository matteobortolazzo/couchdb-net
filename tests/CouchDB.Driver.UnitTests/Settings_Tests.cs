using CouchDB.Driver.Types;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Settings_Tests
    {
        #region Basic

        [Fact]
        public void Creation_Valid()
        {
            using (var client = new CouchClient("http://localhost:5984"))
            {
                Assert.Equal("http://localhost:5984", client.ConnectionString);
            }
        }
        [Fact]
        public void Creation_NullConnectionString()
        {
            var exception = Record.Exception(() =>
            {
                new CouchClient(null);
            });
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        #endregion

        #region Creation Authentication

        [Fact]
        public void Creation_BasicAuthentication()
        {
            using (var client = new CouchClient("http://localhost:5984", s => 
                s.ConfigureBasicAuthentication("root", "relax")))
            {
                Assert.Equal("http://localhost:5984", client.ConnectionString);
            }
        }
        [Fact]
        public void Creation_CookieAuthentication()
        {
            using (var client = new CouchClient("http://localhost:5984", s =>
                s.ConfigureCookieAuthentication("root", "relax")))
            {
                Assert.Equal("http://localhost:5984", client.ConnectionString);
            }
        }

        #endregion

        #region ProperyName

        [Fact]
        public void PropertyName_Camelization()
        {
            using (var client = new CouchClient("http://localhost:5984"))
            {
                var rebels = client.GetDatabase<Rebel>();
                var json = rebels.Where(r => r.Age == 19).ToString();
                Assert.Equal(@"{""selector"":{""age"":19}}", json);
            }
        }
        [Fact]
        public void PropertyName_CamelizationDisabled()
        {
            using (var client = new CouchClient("http://localhost:5984", s =>
                s.SetPropertyCase(PropertyCaseType.None)))
            {
                var rebels = client.GetDatabase<Rebel>();
                var json = rebels.Where(r => r.Age == 19).ToString();
                Assert.Equal(@"{""selector"":{""Age"":19}}", json);
            }
        }
        [Fact]
        public void PropertyName_JsonProperty()
        {
            using (var client = new CouchClient("http://localhost:5984"))
            {
                var rebels = client.GetDatabase<OtherRebel>();
                var json = rebels.Where(r => r.BirthDate == new DateTime(2000, 1, 1)).ToString();
                Assert.Equal(@"{""selector"":{""rebel_bith_date"":""2000-01-01T00:00:00""}}", json);
            }
        }

        #endregion

        #region EntityName

        [Fact]
        public async Task EntityName_Pluralization()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new { Docs = new string[0] });

                using (var client = new CouchClient("http://localhost:5984"))
                {
                    var rebels = client.GetDatabase<Rebel>();
                    var all = await rebels.ToListAsync();

                    httpTest
                        .ShouldHaveCalled("http://localhost:5984/rebels/_find")
                        .WithVerb(HttpMethod.Post);
                }
            }
        }
        [Fact]
        public async Task EntityName_PluralizationDisabled()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new { Docs = new string[0] });

                using (var client = new CouchClient("http://localhost:5984", s => s.DisableEntitisPluralization()))
                {
                    var rebels = client.GetDatabase<Rebel>();
                    var all = await rebels.ToListAsync();

                    httpTest
                        .ShouldHaveCalled("http://localhost:5984/rebel/_find")
                        .WithVerb(HttpMethod.Post);
                }
            }
        }
        [Fact]
        public async Task EntityName_UnderscoreDisabled()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new { Docs = new string[0] });

                using (var client = new CouchClient("http://localhost:5984", s => s
                    .SetEntityCase(EntityCaseType.None)))
                {
                    var rebels = client.GetDatabase<NewRebel>();
                    var all = await rebels.ToListAsync();

                    httpTest
                        .ShouldHaveCalled("http://localhost:5984/newrebels/_find")
                        .WithVerb(HttpMethod.Post);
                }
            }
        }
        [Fact]
        public async Task EntityName_SpecificName()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new { Docs = new string[0] });

                using (var client = new CouchClient("http://localhost:5984"))
                {
                    var rebels = client.GetDatabase<Rebel>("some_rebels");
                    var all = await rebels.ToListAsync();

                    httpTest
                        .ShouldHaveCalled("http://localhost:5984/some_rebels/_find")
                        .WithVerb(HttpMethod.Post);
                }
            }
        }
        [Fact]
        public async Task EntityName_JsonObject()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new { Docs = new string[0] });

                using (var client = new CouchClient("http://localhost:5984"))
                {
                    var rebels = client.GetDatabase<OtherRebel>();
                    var all = await rebels.ToListAsync();

                    httpTest
                        .ShouldHaveCalled("http://localhost:5984/custom_rebels/_find")
                        .WithVerb(HttpMethod.Post);
                }
            }
        }

        #endregion

        #region Utils

        [Fact]
        public void EnsureDatabaseExists()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new [] { "sith" });

                using (var client = new CouchClient("http://localhost:5984", s => s.
                    EnsureDatabaseExists()))
                {
                    client.GetDatabase<Rebel>("yedi");
                    httpTest
                        .ShouldHaveCalled("http://localhost:5984/yedi")
                        .WithVerb(HttpMethod.Put);
                }
            }
        }

        #endregion
    }
}
