using System;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Authentication_Test
    {
        [Fact]
        public async Task None()
        {
            using var httpTest = new HttpTest();
            SetupListResponse(httpTest);

            await using var client = new CouchClient("http://localhost", s => s.UseBasicAuthentication("root", "relax"));

            var rebels = client.GetDatabase<Rebel>();
            var all = await rebels.ToListAsync();

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_find")
                .WithVerb(HttpMethod.Post);
        }
        [Fact]
        public async Task Basic()
        {
            using var httpTest = new HttpTest();
            SetupListResponse(httpTest);

            await using var client = new CouchClient("http://localhost", s => s.UseBasicAuthentication("root", "relax"));
            var rebels = client.GetDatabase<Rebel>();
            var all = await rebels.ToListAsync();

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_find")
                .WithVerb(HttpMethod.Post)
                .WithBasicAuth("root", "relax");
        }
        [Fact]
        public async Task Cookie()
        {
            var token = "cm9vdDo1MEJCRkYwMjq0LO0ylOIwShrgt8y-UkhI-c6BGw";

            using var httpTest = new HttpTest();
            // Cookie response
            var headers = new
            {
                Content_Type = "application/json"
            };
            var cookies = new
            {
                AuthSession = token
            };
            httpTest.RespondWith(string.Empty, 200, headers, cookies);
            SetupListResponse(httpTest);

            await using var client = new CouchClient("http://localhost", s => s.UseCookieAuthentication("root", "relax"));
            var rebels = client.GetDatabase<Rebel>();
            var all = await rebels.ToListAsync();

            var authCookie = httpTest.CallLog
                .Single(c => c.Request.Url.ToString().Contains("_session"))
                .Response.Cookies.Single(c => c.Name == "AuthSession").Value;
            Assert.Equal(token, authCookie);
        }

        [Fact]
        public async Task Proxy()
        {
            using var httpTest = new HttpTest();
            SetupListResponse(httpTest);

            await using var client = new CouchClient("http://localhost", s => s.UseProxyAuthentication("root", new[] { "role1", "role2" }));
            var rebels = client.GetDatabase<Rebel>();
            var all = await rebels.ToListAsync();

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_find")
                .WithVerb(HttpMethod.Post)
                .WithHeader("X-Auth-CouchDB-UserName", "root")
                .WithHeader("X-Auth-CouchDB-Roles", "role1,role2");
        }

        [Fact]
        public async Task Jwt()
        {
            using var httpTest = new HttpTest();
            SetupListResponse(httpTest);

            var jwt = Guid.NewGuid().ToString();
            await using var client = new CouchClient("http://localhost", s => s.UseJwtAuthentication(jwt));
            var rebels = client.GetDatabase<Rebel>();
            var all = await rebels.ToListAsync();

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_find")
                .WithVerb(HttpMethod.Post)
                .WithHeader("Authorization", jwt);
        }

        [Fact]
        public async Task JwtAsync()
        {
            using var httpTest = new HttpTest();
            SetupListResponse(httpTest);

            var jwt = Guid.NewGuid().ToString();
            var jwtTask = Task.FromResult(jwt);

            await using var client = new CouchClient("http://localhost", s => s.UseJwtAuthentication(() => jwtTask));
            var rebels = client.GetDatabase<Rebel>();
            var all = await rebels.ToListAsync();

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_find")
                .WithVerb(HttpMethod.Post)
                .WithHeader("Authorization", jwt);
        }

        private static void SetupListResponse(HttpTest httpTest)
        {
            // ToList
            httpTest.RespondWithJson(new { Docs = new List<string>() });

            // Logout
            httpTest.RespondWithJson(new { ok = true });
        }
    }
}
