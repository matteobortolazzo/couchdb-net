using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.E2ETests.Models;
using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Local;
using CouchDB.Driver.Query.Extensions;
using CouchDB.Driver.Types;
using Xunit;

namespace CouchDB.Driver.E2ETests;

[Trait("Category", "Integration")]
public class DatabaseTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private static Rebel NewRebel(string name) => new(Guid.NewGuid().ToString(), name, "", 19, []);

    [Fact]
    public async Task ChangesFeed()
    {
        for (var i = 1; i <= 20; i++)
        {
            _ = await fixture.Rebels.AddAsync(NewRebel($"Luke_{i}"));
        }

        var lineCount = 0;
        var tokenSource = new CancellationTokenSource();
        await foreach (var unused in fixture.Rebels
                           .GetContinuousChangesAsync(null, null, tokenSource.Token))
        {
            lineCount++;
            switch (lineCount)
            {
                case 20:
                    _ = await fixture.Rebels.AddAsync(NewRebel("Luke_21"), cancellationToken: tokenSource.Token);
                    _ = await fixture.Rebels.AddAsync(NewRebel("Luke_22"), cancellationToken: tokenSource.Token);
                    break;
                case 22:
                    await tokenSource.CancelAsync();
                    break;
            }
        }
    }

    [Fact]
    public async Task Crud()
    {
        var luke = NewRebel("Luke_CRUD");
        var addResponse = await fixture.Rebels.AddAsync(luke);

        luke = luke with { Surname = "Skywalker" };
        addResponse = await fixture.Rebels.ReplaceAsync(luke, luke.Id, addResponse.Rev);

        var findResponse = await fixture.Rebels.FindAsync(luke.Id);
        Assert.NotNull(findResponse);

        luke = findResponse.Document;
        Assert.Equal("Skywalker", luke.Surname);

        await fixture.Rebels.DeleteAsync(luke.Id, addResponse.Rev);
        findResponse = await fixture.Rebels.FindAsync(addResponse.Id!);
        Assert.Null(findResponse);
    }

    [Fact]
    public async Task Crud_Range()
    {
        var luke = NewRebel("Luke_Range_CRUD");

        BulkOperation[] op =
        [
            BulkOperation.Add(luke)
        ];

        var results = await fixture.Rebels.BulkAsync(op);
        var lukeResult = results[0];
        var rebels = await fixture.Rebels.FindManyAsync([lukeResult.Id]);
        Assert.NotEmpty(results);
        luke = rebels[0];

        luke = luke with { Surname = "Skywalker" };
        op =
        [
            BulkOperation.Update(luke, luke.Id, luke.Rev!)
        ];
        results = await fixture.Rebels.BulkAsync(op);

        rebels = await fixture.Rebels.FindManyAsync([luke.Id]);
        Assert.NotEmpty(results);
        luke = rebels[0];
        Assert.Equal("Skywalker", luke.Surname);

        op =
        [
            BulkOperation.Delete(luke.Id, luke.Rev!)
        ];
        await fixture.Rebels.BulkAsync(op);
        rebels = await fixture.Rebels.FindManyAsync([luke.Id]);
        Assert.Empty(rebels);
    }

    [Fact]
    public async Task Crud_Context()
    {
        await using var context = new MyDeathStarContext();
        await context.Rebels.AddAsync(NewRebel("Luke_Context"));
        var result = await context.Rebels.ToListAsync();
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task Crud_Index_Context()
    {
        // Create index
        await using var context = new MyDeathStarContext();
        // Override index
        await using var newContext = new MyDeathStarContext2();

        var indexes = await context.Rebels.GetIndexesAsync();

        Assert.Equal(2, indexes.Count);

        await context.Rebels.AddAsync(new Rebel(Guid.NewGuid().ToString(), "Han", "Solo", 30, []));
        await context.Rebels.AddAsync(new Rebel(Guid.NewGuid().ToString(), "Leia", "Skywalker", 19, []));
        await context.Rebels.AddAsync(new Rebel(Guid.NewGuid().ToString(), "Luke", "Skywalker", 19, []));

        var rebels = await context.Rebels
            .OrderBy(r => r.Surname)
            .ThenBy(r => r.Name)
            .ToListAsync();

        Assert.NotEmpty(rebels);
    }

    [Fact]
    public async Task Crud_SpecialCharacters()
    {
        const string databaseName = "rebel0_$()+/-";
        var rebels = await fixture.Client.GetOrCreateDatabaseAsync<Rebel>(databaseName);

        var luke = NewRebel("Luke_SpecialChars");
        var writeResponse = await rebels.AddAsync(luke);

        luke = luke with { Surname = "Skywalker" };
        await rebels.ReplaceAsync(luke, luke.Id, writeResponse.Rev);

        var findResponse = await rebels.FindAsync(luke.Id);
        Assert.NotNull(findResponse);

        luke = findResponse.Document;
        Assert.Equal("Skywalker", luke.Surname);

        await rebels.DeleteAsync(luke.Id, findResponse.Rev);
        findResponse = await rebels.FindAsync(luke.Id);
        Assert.Null(findResponse);

        await fixture.Client.DeleteDatabaseAsync(databaseName);
    }


    [Fact]
    public async Task Attachment()
    {
        var luke = new Rebel(Guid.NewGuid().ToString(), "Luke_20", "", 19, []))
        var runningPath = Directory.GetCurrentDirectory();

        // Create
        var attachFilePath = Path.Combine(runningPath, "Assets", "luke.txt");
        luke.Attachments.AddOrUpdate(attachFilePath, MediaTypeNames.Text.Plain);
        await fixture.Rebels.AddAsync(luke);

        Assert.Equal("Luke", luke.Name);
        Assert.NotEmpty(luke.Attachments);

        var attachment = luke.Attachments[0];
        Assert.NotNull(attachment);
        Assert.NotNull(attachment.Uri);

        // Download
        var downloadDir = Path.Combine(runningPath, "Assets");
        var downloadFilePath =
            await fixture.Rebels.DownloadAttachmentAsync(attachment, downloadDir, "luke-downloaded.txt");

        Assert.True(File.Exists(downloadFilePath));
        File.Delete(downloadFilePath);

        // Find
        var result = await fixture.Rebels.FindAsync(luke.Id);
        Assert.NotNull(result);

        luke = result;
        Assert.Equal(19, luke.Age);
        attachment = luke.Attachments[0];
        Assert.NotNull(attachment);
        Assert.NotNull(attachment.Uri);
        Assert.NotNull(attachment.Digest);
        Assert.NotNull(attachment.Length);
    }

    [Fact]
    public async Task AttachmentAsStream()
    {
        var luke = new Rebel(Guid.NewGuid().ToString(), "Luke_20", "", 19, []))
        var runningPath = Directory.GetCurrentDirectory();

        var fileOnDiskPath = Path.Combine(runningPath, "Assets", "luke.txt");
        var fileOnDisk = await File.ReadAllBytesAsync(fileOnDiskPath);

        // Create
        var attachFilePath = Path.Combine(runningPath, "Assets", "luke.txt");
        luke.Attachments.AddOrUpdate(attachFilePath, MediaTypeNames.Text.Plain);
        await fixture.Rebels.AddAsync(luke);

        Assert.Equal("Luke", luke.Name);
        Assert.NotEmpty(luke.Attachments);

        var attachment = luke.Attachments[0];
        Assert.NotNull(attachment);
        Assert.NotNull(attachment.Uri);

        // Download
        var responseStream = await fixture.Rebels.DownloadAttachmentAsStreamAsync(attachment);
        var memStream = new MemoryStream();
        await responseStream.CopyToAsync(memStream);
        var fileFromDb = memStream.ToArray();
        var areEqual = Enumerable.SequenceEqual(fileOnDisk, fileFromDb);

        Assert.True(areEqual);
    }

    [Fact]
    public async Task GetInfo()
    {
        var result = await fixture.Rebels.GetInfoAsync();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetRevisionLimit()
    {
        var result = await fixture.Rebels.GetRevisionLimitAsync();

        Assert.True(result > 0);
    }

    [Fact]
    public async Task LocalDocuments()
    {
        var local = fixture.Rebels.LocalDocuments;

        var docId = "传";
        var settings = new RebelSettings
        {
            Id = docId,
            IsActive = true
        };
        await local.CreateOrUpdateAsync(settings, settings.Id);

        settings = await local.GetAsync<RebelSettings>(docId);
        Assert.True(settings.IsActive);

        settings.IsActive = false;
        await local.CreateOrUpdateAsync(settings, settings.Id);
        settings = await local.GetAsync<RebelSettings>(docId);
        Assert.False(settings.IsActive);

        var searchOpt = new LocalDocumentsOptions
        {
            Descending = true,
            Limit = 10,
            Conflicts = true
        };
        var docs = await local.GetAsync(searchOpt);
        var containsId = docs.Select(d => d.Id).Contains("_local/" + docId);
        Assert.True(containsId);
    }

    [Fact]
    public async Task ThrowOnQueryWarning()
    {
        await using var context = new MyDeathStarContextWithQueryWarning();
        // There is an index for Name and Surname so it should not cause a warning
        await context.Rebels.Where(r => r.Name == "Luke" && r.Surname == "Skywalker").ToListAsync();
        try
        {
            // There is no index for Age so it should cause a warning
            await context.Rebels.Where(r => r.Age == 19).ToListAsync();
            Assert.Fail("Expected exception not thrown");
        }
        catch (CouchQueryWarningException e)
        {
            Assert.Equal("No matching index found, create an index to optimize query time.", e.Message);
        }

        var client = new CouchClient("http://localhost:5984", c =>
            c.UseBasicAuthentication("admin", "admin")
                .ThrowOnQueryWarning());
        var crebels = client.GetDatabase<Rebel>();
        // There is an index for Name and Surname so it should not cause a warning
        await crebels.QueryAsync(@"{""selector"":{""$and"":[{""name"":""Luke""},{""surname"":""Skywalker""}]}}");
        try
        {
            // There is no index for Age so it should cause a warning
            await crebels.QueryAsync(@"{""selector"":{""age"":""19""}}");
            Assert.Fail("Expected exception not thrown");
        }
        catch (CouchQueryWarningException e)
        {
            Assert.Equal("No matching index found, create an index to optimize query time.", e.Message);
        }
    }

    [Fact]
    public async Task Index()
    {
        await using var context = new MyDeathStarContext();

        var indexId = await context.Rebels.CreateIndexAsync("AgeIndex", i => i
            .IndexBy(rebel => rebel.Age));
        Assert.NotNull(indexId);

        var indexes = await context.Rebels.GetIndexesAsync();
        Assert.NotEmpty(indexes);
        var ageIndex = indexes.FirstOrDefault(i => i.Name == "AgeIndex");

        Assert.NotNull(ageIndex);
        await context.Rebels.DeleteIndexAsync(ageIndex);
    }

    [Fact]
    public async Task ExecutionStats()
    {
        await using var context = new MyDeathStarContext();

        var rebels = await context.Rebels
            .Where(r => r.Age == 19)
            .IncludeExecutionStats()
            .ToCouchListAsync();

        Assert.NotNull(rebels.ExecutionStats);
        Assert.True(rebels.ExecutionStats.ExecutionTimeMs > 0);
    }
}