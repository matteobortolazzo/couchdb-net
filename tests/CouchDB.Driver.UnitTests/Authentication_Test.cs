using System;
using CouchDB.UnitTests.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.UnitTests._Helpers;
using Xunit;

namespace CouchDB.Driver.UnitTests;

public class Authentication_Test : HttpTests
{
    [Fact]
    public async Task Basic()
    {
        SetupListResponse();

        var cred = new BasicCredentials("root", "relax");
        var client = TestCouchClientFactory.Create(HttpTest, cred);
        var rebels = client.GetDatabase<Rebel>();
        await rebels.ToListAsync();

        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_find")
            .WithVerb(HttpMethod.Post)
            .WithBasicAuth("root", "relax");
    }

    [Fact]
    public async Task Cookie()
    {
        const string token = "cm9vdDo1MEJCRkYwMjq0LO0ylOIwShrgt8y-UkhI-c6BGw";

        // Cookie response
        var headers = new Dictionary<string, string>()
        {
            ["Content_Type"] = "application/json"
        };
        var cookies = new Dictionary<string, string>()
        {
            ["AuthSession"] = token
        };
        HttpTest.RespondWith(string.Empty, 200, headers, cookies);
        SetupListResponse();

        var cred = new CookieCredentials("root", "relax");
        var client = TestCouchClientFactory.Create(HttpTest, cred);
        var rebels = client.GetDatabase<Rebel>();
        await rebels.ToListAsync();

        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_find")
            .WithVerb(HttpMethod.Post)
            .WithHeader("Cookie", $"AuthSession={token}");
    }

    [Fact]
    public async Task Proxy()
    {
        SetupListResponse();

        var cred = new ProxyCredentials("root", ["role1", "role2"]);
        var client = TestCouchClientFactory.Create(HttpTest, cred);
        var rebels = client.GetDatabase<Rebel>();
        await rebels.ToListAsync();

        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_find")
            .WithVerb(HttpMethod.Post)
            .WithHeader("X-Auth-CouchDB-UserName", "root")
            .WithHeader("X-Auth-CouchDB-Roles", "role1,role2");
    }

    [Fact]
    public async Task Jwt()
    {
        SetupListResponse();

        var jwt = Guid.NewGuid().ToString();
        var cred = new JwtCredentials(jwt);
        var client = TestCouchClientFactory.Create(HttpTest, cred);
        var rebels = client.GetDatabase<Rebel>();
        await rebels.ToListAsync();

        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_find")
            .WithVerb(HttpMethod.Post)
            .WithHeader("Authorization", $"Bearer {jwt}");
    }

    [Fact]
    public async Task JwtAsync()
    {
        SetupListResponse();

        var jwt = Guid.NewGuid().ToString();
        var jwtTask = Task.FromResult(jwt);

        var cred = new JwtCredentials(() => jwtTask);
        var client = TestCouchClientFactory.Create(HttpTest, cred);
        var rebels = client.GetDatabase<Rebel>();
        await rebels.ToListAsync();

        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_find")
            .WithVerb(HttpMethod.Post)
            .WithHeader("Authorization", $"Bearer {jwt}");
    }

    private void SetupListResponse()
    {
        // ToList
        HttpTest.RespondWithJson(new { Docs = new List<string>() });

        // Logout
        HttpTest.RespondWithJson(new { ok = true });
    }
}