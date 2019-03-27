using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using System;
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
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new { Docs = new string[0] });

                using (var client = new CouchClient("http://localhost", s => s.UseBasicAuthentication("root", "relax")))
                {
                    var rebels = client.GetDatabase<Rebel>();
                    var all = await rebels.ToListAsync();

                    httpTest
                        .ShouldHaveCalled("http://localhost/rebels/_find")
                        .WithVerb(HttpMethod.Post);
                }
            }
        }
        [Fact]
        public async Task Basic()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(new { Docs = new string[0] });

                using (var client = new CouchClient("http://localhost", s => s.UseBasicAuthentication("root", "relax")))
                {
                    var rebels = client.GetDatabase<Rebel>();
                    var all = await rebels.ToListAsync();

                    httpTest
                        .ShouldHaveCalled("http://localhost/rebels/_find")
                        .WithVerb(HttpMethod.Post)
                        .WithBasicAuth("root", "relax");
                }
            }
        }
        [Fact]
        public async Task Cookie()
        {
            var token = "cm9vdDo1MEJCRkYwMjq0LO0ylOIwShrgt8y-UkhI-c6BGw";

            using (var httpTest = new HttpTest())
            {
                var cookieResponse = new HttpResponseMessage();
                cookieResponse.Headers.Add("Content-Typ", "application/json");
                cookieResponse.Headers.Add("Set-Cookie", $"AuthSession={token}; Version=1; Path=/; HttpOnly");
                cookieResponse.Content = new StringContent("{}");
                httpTest.ResponseQueue.Enqueue(cookieResponse);
                httpTest.RespondWithJson(new { Docs = new string[0] });

                using (var client = new CouchClient("http://localhost", s => s.UseCookieAuthentication("root", "relax")))
                {
                    var rebels = client.GetDatabase<Rebel>();
                    var all = await rebels.ToListAsync();

                    var authCookie = httpTest.CallLog
                        .Single(c => c.Request.RequestUri.ToString().Contains("_session"))
                        .FlurlRequest.Cookies.Single(c => c.Key == "AuthSession").Value;
                    Assert.Equal(token, authCookie.Value);
                }
            }
        }
    }
}
