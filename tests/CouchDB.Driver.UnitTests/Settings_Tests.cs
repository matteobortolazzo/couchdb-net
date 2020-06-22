using CouchDB.Driver.Settings;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using System;
using System.Collections.Generic;
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
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { ok = true });
            using var client = new CouchClient("http://localhost");
            Assert.Equal("http://localhost/", client.DatabaseUri.AbsoluteUri);
        }

        #endregion

        #region Creation Authentication

        [Fact]
        public void Creation_BasicAuthentication()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            using var client = new CouchClient("http://localhost", s => s.UseBasicAuthentication("root", "relax"));
            Assert.Equal("http://localhost/", client.DatabaseUri.AbsoluteUri);
        }

        [Fact]
        public void Creation_CookieAuthentication()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            using var client = new CouchClient("http://localhost", s => s.UseCookieAuthentication("root", "relax"));
            Assert.Equal("http://localhost/", client.DatabaseUri.AbsoluteUri);
        }

        #endregion

        #region ProperyName

        [Fact]
        public void PropertyName_Camelization()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            using var client = new CouchClient("http://localhost");
            var rebels = client.GetDatabase<Rebel>();
            var json = rebels.Where(r => r.Age == 19).ToString();
            Assert.Equal(@"{""selector"":{""age"":19}}", json);
        }

        [Fact]
        public void PropertyName_CamelizationDisabled()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            using var client = new CouchClient("http://localhost", s => s.SetPropertyCase(PropertyCaseType.None));
            var rebels = client.GetDatabase<Rebel>();
            var json = rebels.Where(r => r.Age == 19).ToString();
            Assert.Equal(@"{""selector"":{""Age"":19}}", json);
        }

        [Fact]
        public void PropertyName_JsonProperty()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            using var client = new CouchClient("http://localhost");
            var rebels = client.GetDatabase<OtherRebel>();
            var json = rebels.Where(r => r.BirthDate == new DateTime(2000, 1, 1)).ToString();
            Assert.Equal(@"{""selector"":{""rebel_bith_date"":""2000-01-01T00:00:00""}}", json);
        }

        #endregion

        #region DocumentName

        [Fact]
        public async Task DocumentName_Pluralization()
        {
            using var httpTest = new HttpTest();
            // Find
            httpTest.RespondWithJson(new { Docs = new List<string>() });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            using var client = new CouchClient("http://localhost");
            var rebels = client.GetDatabase<Rebel>();
            var all = await rebels.ToListAsync();

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_find")
                .WithVerb(HttpMethod.Post);
        }

        [Fact]
        public async Task DocumentName_PluralizationDisabled()
        {
            using var httpTest = new HttpTest();
            // Find
            httpTest.RespondWithJson(new { Docs = new List<string>() });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            using var client = new CouchClient("http://localhost", s => s.DisableDocumentPluralization());
            var rebels = client.GetDatabase<Rebel>();
            var all = await rebels.ToListAsync();

            httpTest
                .ShouldHaveCalled("http://localhost/rebel/_find")
                .WithVerb(HttpMethod.Post);
        }

        [Fact]
        public async Task DocumentName_UnderscoreDisabled()
        {
            using var httpTest = new HttpTest();
            // Find
            httpTest.RespondWithJson(new { Docs = new List<string>() });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            using var client = new CouchClient("http://localhost", s => s
                .SetDocumentCase(DocumentCaseType.None));
            var rebels = client.GetDatabase<NewRebel>();
            var all = await rebels.ToListAsync();

            httpTest
                .ShouldHaveCalled("http://localhost/newrebels/_find")
                .WithVerb(HttpMethod.Post);
        }

        [Fact]
        public async Task DocumentName_SpecificName()
        {
            using var httpTest = new HttpTest();
            // Find
            httpTest.RespondWithJson(new { Docs = new List<string>() });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            using var client = new CouchClient("http://localhost");
            var rebels = client.GetDatabase<Rebel>("some_rebels");
            var all = await rebels.ToListAsync();

            httpTest
                .ShouldHaveCalled("http://localhost/some_rebels/_find")
                .WithVerb(HttpMethod.Post);
        }

        [Fact]
        public async Task DocumentName_JsonObject()
        {
            using var httpTest = new HttpTest();
            // Find
            httpTest.RespondWithJson(new { Docs = new List<string>() });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            using var client = new CouchClient("http://localhost");
            var rebels = client.GetDatabase<OtherRebel>();
            var all = await rebels.ToListAsync();

            httpTest
                .ShouldHaveCalled("http://localhost/custom_rebels/_find")
                .WithVerb(HttpMethod.Post);
        }

        #endregion

        #region Utils

        [Fact]
        public void EnsureDatabaseExists()
        {
            using var httpTest = new HttpTest();
            // Databases
            httpTest.RespondWithJson(new[] { "sith" });
            // Create database
            httpTest.RespondWithJson(new { ok = true });
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            using var client = new CouchClient("http://localhost", s => s.
                EnsureDatabaseExists());
            client.GetDatabase<Rebel>("yedi");
            httpTest
                .ShouldHaveCalled("http://localhost/yedi")
                .WithVerb(HttpMethod.Put);
        }

        #endregion
    }
}
