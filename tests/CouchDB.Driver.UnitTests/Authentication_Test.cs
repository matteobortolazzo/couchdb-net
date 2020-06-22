using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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

            using var client = new CouchClient("http://localhost", s => s.UseBasicAuthentication("root", "relax"));

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

            using var client = new CouchClient("http://localhost", s => s.UseBasicAuthentication("root", "relax"));
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
            var cookieResponse = new HttpResponseMessage();
            cookieResponse.Headers.Add("Content-Typ", "application/json");
            cookieResponse.Headers.Add("Set-Cookie", $"AuthSession={token}; Version=1; Path=/; HttpOnly");
            cookieResponse.Content = new StringContent("{}");
            httpTest.ResponseQueue.Enqueue(cookieResponse);
            SetupListResponse(httpTest);

            using var client = new CouchClient("http://localhost", s => s.UseCookieAuthentication("root", "relax"));
            var rebels = client.GetDatabase<Rebel>();
            var all = await rebels.ToListAsync();

            var authCookie = httpTest.CallLog
                .Single(c => c.Request.RequestUri.ToString().Contains("_session"))
                .FlurlRequest.Cookies.Single(c => c.Key == "AuthSession").Value;
            Assert.Equal(token, authCookie.Value);
        }

        [Fact]
        public async Task Proxy()
        {
            using var httpTest = new HttpTest();
            SetupListResponse(httpTest);

            using var client = new CouchClient("http://localhost", s => s.UseProxyAuthentication("root", new[] { "role1", "role2" }));
            var rebels = client.GetDatabase<Rebel>();
            var all = await rebels.ToListAsync();

            httpTest
                .ShouldHaveCalled("http://localhost/rebels/_find")
                .WithVerb(HttpMethod.Post)
                .WithHeader("X-Auth-CouchDB-UserName", "root")
                .WithHeader("X-Auth-CouchDB-Roles", "role1,role2");
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
