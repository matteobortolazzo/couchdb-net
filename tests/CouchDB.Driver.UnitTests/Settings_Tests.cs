using CouchDB.UnitTests.Models;
using Flurl.Http.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Options;
using Newtonsoft.Json;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Settings_Tests
    {
        #region Basic

        [Fact]
        public async Task Creation_Valid()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { ok = true });
            await using var client = new CouchClient("http://localhost");
            Assert.Equal("http://localhost/", client.Endpoint.AbsoluteUri);
        }

        #endregion

        #region Creation Authentication

        [Fact]
        public async Task Creation_BasicAuthentication()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost", s => s.UseBasicAuthentication("root", "relax"));
            Assert.Equal("http://localhost/", client.Endpoint.AbsoluteUri);
        }

        [Fact]
        public async Task Creation_CookieAuthentication()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost", s => s.UseCookieAuthentication("root", "relax"));
            Assert.Equal("http://localhost/", client.Endpoint.AbsoluteUri);
        }

        #endregion

        #region ProperyName

        [Fact]
        public async Task PropertyName_Camelization()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            var rebels = client.GetDatabase<Rebel>();
            var json = rebels.Where(r => r.Age == 19).ToString();
            Assert.Equal(@"{""selector"":{""age"":19}}", json);
        }

        [Fact]
        public async Task PropertyName_CamelizationDisabled()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost", s => s.SetPropertyCase(PropertyCaseType.None));
            var rebels = client.GetDatabase<Rebel>();
            var json = rebels.Where(r => r.Age == 19).ToString();
            Assert.Equal(@"{""selector"":{""Age"":19}}", json);
        }

        [Fact]
        public async Task PropertyName_JsonProperty()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
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

            await using var client = new CouchClient("http://localhost");
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

            await using var client = new CouchClient("http://localhost", s => s.DisableDocumentPluralization());
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

            await using var client = new CouchClient("http://localhost", s => s
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

            await using var client = new CouchClient("http://localhost");
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

            await using var client = new CouchClient("http://localhost");
            var rebels = client.GetDatabase<OtherRebel>();
            var all = await rebels.ToListAsync();

            httpTest
                .ShouldHaveCalled("http://localhost/custom_rebels/_find")
                .WithVerb(HttpMethod.Post);
        }

        #endregion

        #region Null Value Handling

        [Fact]
        public async Task PropertyNullValueHandling_NotSet()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost");
            var rebels = client.GetDatabase<Rebel>();
            await rebels.AddAsync(new Rebel());

            var call = httpTest.CallLog.First();
            Assert.NotNull(call);
            Assert.Equal(@"{""age"":0,""isJedi"":false,""species"":0,""guid"":""00000000-0000-0000-0000-000000000000"",""_attachments"":{}}", call.RequestBody);
        }

        [Fact]
        public async Task PropertyNullValueHandling_Includes()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost", x => x.SetJsonNullValueHandling(NullValueHandling.Include));
            var rebels = client.GetDatabase<Rebel>();
            await rebels.AddAsync(new Rebel());

            var call = httpTest.CallLog.First();
            Assert.NotNull(call);
            Assert.Equal(@"{""name"":null,""surname"":null,""age"":0,""isJedi"":false,""species"":0,""guid"":""00000000-0000-0000-0000-000000000000"",""skills"":null,""battles"":null,""vehicle"":null,""_attachments"":{}}", call.RequestBody);
        }

        [Fact]
        public async Task PropertyNullValueHandling_Ignore()
        {
            using var httpTest = new HttpTest();
            // Logout
            httpTest.RespondWithJson(new { ok = true });

            await using var client = new CouchClient("http://localhost", x => x.SetJsonNullValueHandling(NullValueHandling.Ignore));
            var rebels = client.GetDatabase<Rebel>();
            await rebels.AddAsync(new Rebel());

            var call = httpTest.CallLog.First();
            Assert.NotNull(call);
            Assert.Equal(@"{""age"":0,""isJedi"":false,""species"":0,""guid"":""00000000-0000-0000-0000-000000000000"",""_attachments"":{}}", call.RequestBody);
        }

        #endregion
    }
}
