using CouchDB.UnitTests.Models;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CouchDB.Driver.Types;
using CouchDB.Driver.UnitTests._Helpers;
using Xunit;

namespace CouchDB.Driver.UnitTests;

public class Attachments_Tests : HttpTest
{
    [Fact]
    public async Task DownloadAttachment()
    {
        httpTest.RespondWithJson(new
        {
            Id = "1",
            Ok = true,
            Rev = "xxx",
            Attachments = new Dictionary<string, object>
            {
                { "luke.txt", new { ContentType = "text/plain" } }
            }
        });

        httpTest.RespondWithJson(new
        {
            Id = "1",
            Ok = true,
            Rev = "xxx2",
        });

        var r = new Rebel { Id = "1", Name = "Luke" };
        var attachFile = Path.Combine("Assets", "luke.txt");

        var addResult = await _rebels.CreateItemAsync(r, new CreateItemRequestOptions
        {
            Attachments =
            [
                new CreateItemAttachment(attachFile)
            ]
        });

        var newPath = await _rebels.DownloadAttachmentAsync("luke.txt", addResult.Id, addResult.Rev, "anyfolder");

        httpTest
            .ShouldHaveCalled("http://localhost/rebels/1")
            .WithVerb(HttpMethod.Put);
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/1/luke.txt")
            .WithVerb(HttpMethod.Put)
            .WithHeader("If-Match", "xxx");
        httpTest
            .ShouldHaveCalled("http://localhost/rebels/1/luke.txt")
            .WithVerb(HttpMethod.Get)
            .WithHeader("If-Match", "xxx2");

        var newAttachFile = Path.Combine("anyfolder", "luke.txt");
        Assert.Equal(newAttachFile, newPath);
    }
}