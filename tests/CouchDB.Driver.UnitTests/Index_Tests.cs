using System.Net.Http;
using System.Threading.Tasks;
using CouchDB.Driver.Indexes;
using CouchDB.Driver.UnitTests._Helpers;
using Xunit;

namespace CouchDB.Driver.UnitTests;

public class Index_Tests : HttpTests
{
    [Fact]
    public async Task CreateIndex()
    {
        HttpTest.RespondWithJson(new
        {
            result = "created",
            id = "_design/skywalkers_ddoc",
            name = "skywalkers"
        });

        await Rebels.CreateIndexAsync("skywalkers", b => b
            .IndexBy(r => r.Surname));

        var expectedBody =
            "{\"index\":{\"fields\":[\"surname\"]},\"name\":\"skywalkers\",\"type\":\"json\"}";
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_index")
            .WithRequestBody(expectedBody)
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task CreateIndex_WithOptions()
    {
        HttpTest.RespondWithJson(new
        {
            result = "created",
            id = "_design/skywalkers_ddoc",
            name = "skywalkers"
        });

        await Rebels.CreateIndexAsync("skywalkers", b => b
                .IndexByDescending(r => r.Surname)
                .ThenByDescending(r => r.Name),
            new IndexOptions()
            {
                DesignDocument = "skywalkers_ddoc",
                Partitioned = true
            });


        var expectedBody =
            "{\"index\":{\"fields\":[{\"surname\":\"desc\"},{\"name\":\"desc\"}]},\"name\":\"skywalkers\",\"type\":\"json\",\"ddoc\":\"skywalkers_ddoc\",\"partitioned\":true}";
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_index")
            .WithRequestBody(expectedBody)
            .WithVerb(HttpMethod.Post);
    }

    [Fact]
    public async Task CreateIndex_Partial()
    {
        HttpTest.RespondWithJson(new
        {
            result = "created",
            id = "_design/skywalkers_ddoc",
            name = "skywalkers"
        });

        await Rebels.CreateIndexAsync("skywalkers", b => b
                .IndexBy(r => r.Surname)
                .Where(r => r.Surname == "Skywalker"),
            new IndexOptions()
            {
                DesignDocument = "skywalkers_ddoc",
                Partitioned = true
            });


        var expectedBody =
            "{\"index\":{\"partial_filter_selector\":{\"surname\":\"Skywalker\"},\"fields\":[\"surname\"]},\"name\":\"skywalkers\",\"type\":\"json\",\"ddoc\":\"skywalkers_ddoc\",\"partitioned\":true}";
        HttpTest
            .ShouldHaveCalled("http://localhost/rebels/_index")
            .WithRequestBody(expectedBody)
            .WithVerb(HttpMethod.Post);
    }
}