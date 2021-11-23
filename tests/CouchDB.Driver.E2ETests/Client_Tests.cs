using CouchDB.Driver.E2E.Models;
using CouchDB.Driver.Types;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver.E2ETests;
using CouchDB.Driver.E2ETests._Models;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Local;
using CouchDB.Driver.Query.Extensions;
using Xunit;

namespace CouchDB.Driver.E2E
{
    [Trait("Category", "Integration")]
    public class ClientTests: IAsyncLifetime
    {
        private ICouchClient _client;
        private ICouchDatabase<Rebel> _rebels;

        public async Task InitializeAsync()
        {
            _client = new CouchClient("http://localhost:5984", c =>
                c.UseBasicAuthentication("admin", "admin"));
            _rebels = await _client.GetOrCreateDatabaseAsync<Rebel>();
        }

        public async Task DisposeAsync()
        {
            await _client.DeleteDatabaseAsync<Rebel>();
            await _client.DisposeAsync();
        }

        [Fact]
        public async Task ChangesFeed()
        {
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_1", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_2", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_3", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_4", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_5", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_6", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_7", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_8", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_9", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_10", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_11", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_12", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_13", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_14", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_15", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_16", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_17", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_18", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_19", Age = 19 });
            _ = await _rebels.AddAsync(new Rebel { Name = "Luke_20", Age = 19 });

            var lineCount = 0;
            var tokenSource = new CancellationTokenSource();
            await foreach (var l in _rebels.GetContinuousChangesAsync(null, null, tokenSource.Token))
            {
                lineCount++;
                if (lineCount == 20)
                {
                    _ = await _rebels.AddAsync(new Rebel { Name = "Luke_11", Age = 19 });
                    _ = await _rebels.AddAsync(new Rebel { Name = "Luke_12", Age = 19 });
                }

                if (lineCount == 22)
                {
                    tokenSource.Cancel();
                }
            }
        }

        [Fact]
        public async Task Crud()
        {
            Rebel luke = await _rebels.AddAsync(new Rebel { Name = "Luke", Age = 19 });
            Assert.Equal("Luke", luke.Name);

            luke.Surname = "Skywalker";
            luke = await _rebels.AddOrUpdateAsync(luke);
            Assert.Equal("Skywalker", luke.Surname);

            luke = await _rebels.FindAsync(luke.Id);
            Assert.Equal(19, luke.Age);

            await _rebels.RemoveAsync(luke);
            luke = await _rebels.FindAsync(luke.Id);
            Assert.Null(luke);
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
            var rebels = await _client.GetOrCreateDatabaseAsync<Rebel>(databaseName);

            Rebel luke = await rebels.AddAsync(new Rebel { Name = "Luke", Age = 19 });
            Assert.Equal("Luke", luke.Name);

            luke.Surname = "Skywalker";
            luke = await rebels.AddOrUpdateAsync(luke);
            Assert.Equal("Skywalker", luke.Surname);

            luke = await rebels.FindAsync(luke.Id);
            Assert.Equal(19, luke.Age);

            await rebels.RemoveAsync(luke);
            luke = await rebels.FindAsync(luke.Id);
            Assert.Null(luke);

            await _client.DeleteDatabaseAsync(databaseName);
        }

        [Fact]
        public async Task Users()
        {
            var users = await _client.GetOrCreateUsersDatabaseAsync();
            
            CouchUser luke = await users.AddAsync(new CouchUser(name: "luke", password: "lasersword"));
            Assert.Equal("luke", luke.Name);

            luke = await users.FindAsync(luke.Id);
            Assert.Equal("luke", luke.Name);

            luke = await users.ChangeUserPassword(luke, "r2d2");

            await users.RemoveAsync(luke);
            luke = await users.FindAsync(luke.Id);
            Assert.Null(luke);

            await _client.DeleteDatabaseAsync<CouchUser>();
        }

        [Fact]
        public async Task Attachment()
        {
            var luke = new Rebel { Name = "Luke", Age = 19 };
            var runningPath = Directory.GetCurrentDirectory();

            // Create
            luke.Attachments.AddOrUpdate($@"{runningPath}\Assets\luke.txt", MediaTypeNames.Text.Plain);
            luke = await _rebels.AddAsync(luke);

            Assert.Equal("Luke", luke.Name);
            Assert.NotEmpty(luke.Attachments);

            CouchAttachment attachment = luke.Attachments.First();
            Assert.NotNull(attachment);
            Assert.NotNull(attachment.Uri);

            // Download
            var downloadFilePath = await _rebels.DownloadAttachmentAsync(attachment, $@"{runningPath}\Assets", "luke-downloaded.txt");

            Assert.True(File.Exists(downloadFilePath));
            File.Delete(downloadFilePath);

            // Find
            luke = await _rebels.FindAsync(luke.Id);
            Assert.Equal(19, luke.Age);
            attachment = luke.Attachments.First();
            Assert.NotNull(attachment);
            Assert.NotNull(attachment.Uri);
            Assert.NotNull(attachment.Digest);
            Assert.NotNull(attachment.Length);

            // Update
            luke.Surname = "Skywalker";
            luke = await _rebels.AddOrUpdateAsync(luke);
            Assert.Equal("Skywalker", luke.Surname);
        }

        [Fact]
        public async Task AttachmentAsStream()
        {
            var luke = new Rebel { Name = "Luke", Age = 19 };
            var runningPath = Directory.GetCurrentDirectory();

            var fileOnDisk = File.ReadAllBytes($@"{runningPath}\Assets\luke.txt");

            // Create
            luke.Attachments.AddOrUpdate($@"{runningPath}\Assets\luke.txt", MediaTypeNames.Text.Plain);
            luke = await _rebels.AddAsync(luke);

            Assert.Equal("Luke", luke.Name);
            Assert.NotEmpty(luke.Attachments);

            CouchAttachment attachment = luke.Attachments.First();
            Assert.NotNull(attachment);
            Assert.NotNull(attachment.Uri);

            // Download
            var responseStream = await _rebels.DownloadAttachmentAsStreamAsync(attachment);
            var memStream = new MemoryStream();
            responseStream.CopyTo(memStream);
            var fileFromDb = memStream.ToArray();
            var areEqual = fileOnDisk.SequenceEqual(fileFromDb);

            Assert.True(areEqual);
        }

        [Fact]
        public async Task LocalDocuments()
        {
            var local = _rebels.LocalDocuments;

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
    }
}
