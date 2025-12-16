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
using Xunit;

namespace CouchDB.Driver.E2ETests;

[Trait("Category", "Integration")]
public class DatabaseTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task ChangesFeed()
    {
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_1", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_2", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_3", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_4", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_5", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_6", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_7", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_8", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_9", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_10", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_11", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_12", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_13", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_14", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_15", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_16", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_17", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_18", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_19", Age = 19 });
        _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_20", Age = 19 });

        var lineCount = 0;
        var tokenSource = new CancellationTokenSource();
        await foreach (var unused in fixture.Rebels
                           .GetContinuousChangesAsync(null, null, tokenSource.Token))
        {
            lineCount++;
            if (lineCount == 20)
            {
                _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_11", Age = 19 },
                    cancellationToken: tokenSource.Token);
                _ = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke_12", Age = 19 },
                    cancellationToken: tokenSource.Token);
            }

            if (lineCount == 22)
            {
                await tokenSource.CancelAsync();
            }
        }
    }

    [Fact]
    public async Task Crud()
    {
        Rebel luke = await fixture.Rebels.AddAsync(new Rebel { Name = "Luke", Age = 19 });
        Assert.Equal("Luke", luke.Name);

        luke.Surname = "Skywalker";
        luke = await fixture.Rebels.AddOrUpdateAsync(luke);
        Assert.Equal("Skywalker", luke.Surname);

        luke = await fixture.Rebels.FindAsync(luke.Id);
        Assert.Equal(19, luke.Age);

        await fixture.Rebels.DeleteAsync(luke);
        luke = await fixture.Rebels.FindAsync(luke.Id);
        Assert.Null(luke);
    }

    [Fact]
    public async Task Crud_Range()
    {
        var luke = new Rebel { Name = "Luke", Age = 19 };
        var results = await fixture.Rebels.AddOrUpdateRangeAsync([luke]);
        luke = results[0];
        Assert.Equal("Luke", luke.Name);

        luke.Surname = "Skywalker";
        results = await fixture.Rebels.AddOrUpdateRangeAsync([luke]);
        luke = results[0];
        Assert.Equal("Luke", luke.Name);

        results = await fixture.Rebels.FindManyAsync([luke.Id]);
        Assert.NotEmpty(results);

        await fixture.Rebels.DeleteRangeAsync([luke]);
        results = await fixture.Rebels.FindManyAsync([luke.Id]);
        Assert.Empty(results);
    }

    [Fact]
    public async Task Crud_Context()
    {
        await using var context = new MyDeathStarContext();
        var luke = await context.Rebels.AddAsync(new Rebel { Name = "Luke", Age = 19 });
        Assert.Equal("Luke", luke.Name);
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

        await context.Rebels.AddAsync(new Rebel { Name = "Han", Age = 30, Surname = "Solo" });
        await context.Rebels.AddAsync(new Rebel { Name = "Leia", Age = 19, Surname = "Skywalker" });
        await context.Rebels.AddAsync(new Rebel { Name = "Luke", Age = 19, Surname = "Skywalker" });

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

        Rebel luke = await rebels.AddAsync(new Rebel { Name = "Luke", Age = 19 });
        Assert.Equal("Luke", luke.Name);

        luke.Surname = "Skywalker";
        luke = await rebels.AddOrUpdateAsync(luke);
        Assert.Equal("Skywalker", luke.Surname);

        var result = await rebels.FindAsync(luke.Id);
        Assert.NotNull(result);
        
        luke = result;
        Assert.Equal(19, luke.Age);

        await rebels.DeleteAsync(luke);
        luke = await rebels.FindAsync(luke.Id);
        Assert.Null(luke);

        await fixture.Client.DeleteDatabaseAsync(databaseName);
    }


    [Fact]
    public async Task Attachment()
    {
        var luke = new Rebel { Name = "Luke", Age = 19 };
        var runningPath = Directory.GetCurrentDirectory();

        // Create
        var attachFilePath = Path.Combine(runningPath, "Assets", "luke.txt");
        luke.Attachments.AddOrUpdate(attachFilePath, MediaTypeNames.Text.Plain);
        luke = await fixture.Rebels.AddAsync(luke);

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

        // Update
        luke.Surname = "Skywalker";
        luke = await fixture.Rebels.AddOrUpdateAsync(luke);
        Assert.Equal("Skywalker", luke.Surname);
    }

    [Fact]
    public async Task AttachmentAsStream()
    {
        var luke = new Rebel { Name = "Luke", Age = 19 };
        var runningPath = Directory.GetCurrentDirectory();

        var fileOnDiskPath = Path.Combine(runningPath, "Assets", "luke.txt");
        var fileOnDisk = await File.ReadAllBytesAsync(fileOnDiskPath);

        // Create
        var attachFilePath = Path.Combine(runningPath, "Assets", "luke.txt");
        luke.Attachments.AddOrUpdate(attachFilePath, MediaTypeNames.Text.Plain);
        luke = await fixture.Rebels.AddAsync(luke);

        Assert.Equal("Luke", luke.Name);
        Assert.NotEmpty(luke.Attachments);

        var attachment = luke.Attachments.First();
        Assert.NotNull(attachment);
        Assert.NotNull(attachment.Uri);

        // Download
        var responseStream = await fixture.Rebels.DownloadAttachmentAsStreamAsync(attachment);
        var memStream = new MemoryStream();
        await responseStream.CopyToAsync(memStream);
        var fileFromDb = memStream.ToArray();
        var areEqual = fileOnDisk.SequenceEqual(fileFromDb);

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
        await local.CreateOrUpdateAsync(settings);

        settings = await local.GetAsync<RebelSettings>(docId);
        Assert.True(settings.IsActive);

        settings.IsActive = false;
        await local.CreateOrUpdateAsync(settings);
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