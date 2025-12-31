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

public class Authentication_Test : HttpTest
{
    [Fact]
    public async Task None()
    {
        SetupListResponse();

        await using var client = new CouchClient("http://localhost",
            new BasicCredentials("root", "relax"));

        var rebels = client.GetDatabase<Rebel>();
        await rebels.ToListAsync();

        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_find")
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task Basic()
    {
        SetupListResponse();

        await using var client = new CouchClient("http://localhost",
            new BasicCredentials("root", "relax"));
        var rebels = client.GetDatabase<Rebel>();
        await rebels.ToListAsync();

        httpTest
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
        httpTest.RespondWith(string.Empty, 200, headers, cookies);
        SetupListResponse();

        await using var client = new CouchClient("http://localhost",
            new CookieCredentials("root", "relax"));
        var rebels = client.GetDatabase<Rebel>();
        await rebels.ToListAsync();

        var sessionRequest = httpTest.CallLog
            .Single(c => c.Request.RequestUri!.ToString().Contains("_session"));
        var authCookie = sessionRequest.Request
            .Headers.GetValues("Cookie")
            .FirstOrDefault();
        Assert.Equal($"AuthSession={token}", authCookie);
    }

    [Fact]
    public async Task Proxy()
    {
        SetupListResponse();

        await using var client = new CouchClient("http://localhost",
            new ProxyCredentials("root", ["role1", "role2"]));
        var rebels = client.GetDatabase<Rebel>();
        await rebels.ToListAsync();

        httpTest
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
        await using var client = new CouchClient("http://localhost", new JwtCredentials(jwt));
        var rebels = client.GetDatabase<Rebel>();
        await rebels.ToListAsync();

        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_find")
            .WithVerb(HttpMethod.Post)
            .WithHeader("Authorization", jwt);
    }

    [Fact]
    public async Task JwtAsync()
    {
        SetupListResponse();

        var jwt = Guid.NewGuid().ToString();
        var jwtTask = Task.FromResult(jwt);

        await using var client = new CouchClient("http://localhost", new JwtCredentials(() => jwtTask));
        var rebels = client.GetDatabase<Rebel>();
        await rebels.ToListAsync();

        httpTest
            .ShouldHaveCalled("http://localhost/rebels/_find")
            .WithVerb(HttpMethod.Post)
            .WithHeader("Authorization", jwt);
    }

    private void SetupListResponse()
    {
        // ToList
        httpTest.RespondWithJson(new { Docs = new List<string>() });

        // Logout
        httpTest.RespondWithJson(new { ok = true });
    }
}