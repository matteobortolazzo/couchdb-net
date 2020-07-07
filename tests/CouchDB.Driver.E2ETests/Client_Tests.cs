using CouchDB.Driver.E2E.Models;
using CouchDB.Driver.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using CouchDB.Driver.E2ETests._Models;
using CouchDB.Driver.Local;
using Xunit;

namespace CouchDB.Driver.E2E
{
    [Trait("Category", "Integration")]
    public class ClientTests
    {
        [Fact]
        public async Task Crud()
        {
            await using var client = new CouchClient("http://localhost:5984", c =>
                c.UseBasicAuthentication("admin", "admin"));
            IEnumerable<string> dbs = await client.GetDatabasesNamesAsync();
            ICouchDatabase<Rebel> rebels = client.GetDatabase<Rebel>();

            if (dbs.Contains(rebels.Database))
            {
                await client.DeleteDatabaseAsync<Rebel>();
            }

            rebels = await client.CreateDatabaseAsync<Rebel>();

            Rebel luke = await rebels.CreateAsync(new Rebel { Name = "Luke", Age = 19 });
            Assert.Equal("Luke", luke.Name);

            luke.Surname = "Skywalker";
            luke = await rebels.CreateOrUpdateAsync(luke);
            Assert.Equal("Skywalker", luke.Surname);

            luke = await rebels.FindAsync(luke.Id);
            Assert.Equal(19, luke.Age);

            await rebels.DeleteAsync(luke);
            luke = await rebels.FindAsync(luke.Id);
            Assert.Null(luke);

            await client.DeleteDatabaseAsync<Rebel>();
        }

        [Fact]
        public async Task Crud_SpecialCharacters()
        {
            var databaseName = "rebel0_$()+/-";

            await using var client = new CouchClient("http://localhost:5984", c =>
                c.UseBasicAuthentication("admin", "admin"));
            IEnumerable<string> dbs = await client.GetDatabasesNamesAsync();
            ICouchDatabase<Rebel> rebels = client.GetDatabase<Rebel>(databaseName);

            if (dbs.Contains(rebels.Database))
            {
                await client.DeleteDatabaseAsync(databaseName);
            }

            rebels = await client.GetOrCreateDatabaseAsync<Rebel>(databaseName);

            Rebel luke = await rebels.CreateAsync(new Rebel { Name = "Luke", Age = 19 });
            Assert.Equal("Luke", luke.Name);

            luke.Surname = "Skywalker";
            luke = await rebels.CreateOrUpdateAsync(luke);
            Assert.Equal("Skywalker", luke.Surname);

            luke = await rebels.FindAsync(luke.Id);
            Assert.Equal(19, luke.Age);

            await rebels.DeleteAsync(luke);
            luke = await rebels.FindAsync(luke.Id);
            Assert.Null(luke);

            await client.DeleteDatabaseAsync(databaseName);
        }

        [Fact]
        public async Task Users()
        {
            await using var client = new CouchClient("http://localhost:5984", c =>
                c.UseBasicAuthentication("admin", "admin"));
            IEnumerable<string> dbs = await client.GetDatabasesNamesAsync();
            ICouchDatabase<CouchUser> users = client.GetUsersDatabase();

            if (!dbs.Contains(users.Database))
            {
                users = await client.CreateDatabaseAsync<CouchUser>();
            }

            CouchUser luke = await users.CreateAsync(new CouchUser(name: "luke", password: "lasersword"));
            Assert.Equal("luke", luke.Name);

            luke = await users.FindAsync(luke.Id);
            Assert.Equal("luke", luke.Name);

            await users.DeleteAsync(luke);
            luke = await users.FindAsync(luke.Id);
            Assert.Null(luke);

            await client.DeleteDatabaseAsync<CouchUser>();
        }

        [Fact]
        public async Task Attachment()
        {
            await using var client = new CouchClient("http://localhost:5984", c =>
                c.UseBasicAuthentication("admin", "admin"));
            IEnumerable<string> dbs = await client.GetDatabasesNamesAsync();
            ICouchDatabase<Rebel> rebels = client.GetDatabase<Rebel>();

            if (dbs.Contains(rebels.Database))
            {
                await client.DeleteDatabaseAsync<Rebel>();
            }

            rebels = await client.CreateDatabaseAsync<Rebel>();

            var luke = new Rebel { Name = "Luke", Age = 19 };
            var runningPath = Directory.GetCurrentDirectory();

            // Create
            luke.Attachments.AddOrUpdate($@"{runningPath}\Assets\luke.txt", MediaTypeNames.Text.Plain);
            luke = await rebels.CreateAsync(luke);

            Assert.Equal("Luke", luke.Name);
            Assert.NotEmpty(luke.Attachments);

            CouchAttachment attachment = luke.Attachments.First();
            Assert.NotNull(attachment);
            Assert.NotNull(attachment.Uri);

            // Download
            var downloadFilePath = await rebels.DownloadAttachmentAsync(attachment, $@"{runningPath}\Assets", "luke-downloaded.txt");

            Assert.True(File.Exists(downloadFilePath));
            File.Delete(downloadFilePath);

            // Find
            luke = await rebels.FindAsync(luke.Id);
            Assert.Equal(19, luke.Age);
            attachment = luke.Attachments.First();
            Assert.NotNull(attachment);
            Assert.NotNull(attachment.Uri);
            Assert.NotNull(attachment.Digest);
            Assert.NotNull(attachment.Length);

            // Update
            luke.Surname = "Skywalker";
            luke = await rebels.CreateOrUpdateAsync(luke);
            Assert.Equal("Skywalker", luke.Surname);

            await client.DeleteDatabaseAsync<Rebel>();
        }

        [Fact]
        public async Task LocalDocuments()
        {
            await using var client = new CouchClient("http://localhost:5984", c =>
                c.UseBasicAuthentication("admin", "admin"));

            IEnumerable<string> dbs = await client.GetDatabasesNamesAsync();
            ICouchDatabase<Rebel> users = client.GetDatabase<Rebel>();

            if (!dbs.Contains(users.Database))
            {
                users = await client.CreateDatabaseAsync<Rebel>();
            }

            var local = users.LocalDocuments;

            var docId = "rebel_settings";
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

            var docs = await local.GetAsync();
            var containsId = docs.Select(d => d.Id).Contains("_local/" + docId);
            Assert.True(containsId);

            await client.DeleteDatabaseAsync<Rebel>();
        }
    }
}
