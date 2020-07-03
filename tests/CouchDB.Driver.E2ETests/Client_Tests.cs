using CouchDB.Driver.E2E.Models;
using CouchDB.Driver.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Driver.E2E
{
    [Trait("Category", "Integration")]
    public class ClientTests
    {
        [Fact]
        public async Task Crud()
        {
            await using var client = new CouchClient("http://localhost:5984");
            IEnumerable<string> dbs = await client.GetDatabasesNamesAsync().ConfigureAwait(false);
            ICouchDatabase<Rebel> rebels = client.GetDatabase<Rebel>();

            if (dbs.Contains(rebels.Database))
            {
                await client.DeleteDatabaseAsync<Rebel>().ConfigureAwait(false);
            }

            rebels = await client.CreateDatabaseAsync<Rebel>().ConfigureAwait(false);

            Rebel luke = await rebels.CreateAsync(new Rebel { Name = "Luke", Age = 19 }).ConfigureAwait(false);
            Assert.Equal("Luke", luke.Name);

            luke.Surname = "Skywalker";
            luke = await rebels.CreateOrUpdateAsync(luke).ConfigureAwait(false);
            Assert.Equal("Skywalker", luke.Surname);

            luke = await rebels.FindAsync(luke.Id).ConfigureAwait(false);
            Assert.Equal(19, luke.Age);

            await rebels.DeleteAsync(luke).ConfigureAwait(false);
            luke = await rebels.FindAsync(luke.Id).ConfigureAwait(false);
            Assert.Null(luke);

            await client.DeleteDatabaseAsync<Rebel>().ConfigureAwait(false);
        }

        [Fact]
        public async Task Crud_SpecialCharacters()
        {
            var databaseName = "rebel0_$()+/-";

            await using var client = new CouchClient("http://localhost:5984");
            IEnumerable<string> dbs = await client.GetDatabasesNamesAsync().ConfigureAwait(false);
            ICouchDatabase<Rebel> rebels = client.GetDatabase<Rebel>(databaseName);

            if (dbs.Contains(rebels.Database))
            {
                await client.DeleteDatabaseAsync(databaseName).ConfigureAwait(false);
            }

            rebels = await client.GetOrCreateDatabaseAsync<Rebel>(databaseName).ConfigureAwait(false);

            Rebel luke = await rebels.CreateAsync(new Rebel { Name = "Luke", Age = 19 }).ConfigureAwait(false);
            Assert.Equal("Luke", luke.Name);

            luke.Surname = "Skywalker";
            luke = await rebels.CreateOrUpdateAsync(luke).ConfigureAwait(false);
            Assert.Equal("Skywalker", luke.Surname);

            luke = await rebels.FindAsync(luke.Id).ConfigureAwait(false);
            Assert.Equal(19, luke.Age);

            await rebels.DeleteAsync(luke).ConfigureAwait(false);
            luke = await rebels.FindAsync(luke.Id).ConfigureAwait(false);
            Assert.Null(luke);

            await client.DeleteDatabaseAsync(databaseName).ConfigureAwait(false);
        }

        [Fact]
        public async Task Users()
        {
            await using var client = new CouchClient("http://localhost:5984");
            IEnumerable<string> dbs = await client.GetDatabasesNamesAsync().ConfigureAwait(false);
            ICouchDatabase<CouchUser> users = client.GetUsersDatabase();

            if (!dbs.Contains(users.Database))
            {
                users = await client.CreateDatabaseAsync<CouchUser>().ConfigureAwait(false);
            }

            CouchUser luke = await users.CreateAsync(new CouchUser(name: "luke", password: "lasersword")).ConfigureAwait(false);
            Assert.Equal("luke", luke.Name);

            luke = await users.FindAsync(luke.Id).ConfigureAwait(false);
            Assert.Equal("luke", luke.Name);

            await users.DeleteAsync(luke).ConfigureAwait(false);
            luke = await users.FindAsync(luke.Id).ConfigureAwait(false);
            Assert.Null(luke);

            await client.DeleteDatabaseAsync<CouchUser>().ConfigureAwait(false);
        }

        [Fact]
        public async Task Attachment()
        {
            await using var client = new CouchClient("http://localhost:5984");
            IEnumerable<string> dbs = await client.GetDatabasesNamesAsync().ConfigureAwait(false);
            ICouchDatabase<Rebel> rebels = client.GetDatabase<Rebel>();

            if (dbs.Contains(rebels.Database))
            {
                await client.DeleteDatabaseAsync<Rebel>().ConfigureAwait(false);
            }

            rebels = await client.CreateDatabaseAsync<Rebel>().ConfigureAwait(false);

            var luke = new Rebel { Name = "Luke", Age = 19 };
            var runningPath = Directory.GetCurrentDirectory();

            // Create
            luke.Attachments.AddOrUpdate($@"{runningPath}\Assets\luke.txt", MediaTypeNames.Text.Plain);
            luke = await rebels.CreateAsync(luke).ConfigureAwait(false);

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
            luke = await rebels.FindAsync(luke.Id).ConfigureAwait(false);
            Assert.Equal(19, luke.Age);
            attachment = luke.Attachments.First();
            Assert.NotNull(attachment);
            Assert.NotNull(attachment.Uri);
            Assert.NotNull(attachment.Digest);
            Assert.NotNull(attachment.Length);

            // Update
            luke.Surname = "Skywalker";
            luke = await rebels.CreateOrUpdateAsync(luke).ConfigureAwait(false);
            Assert.Equal("Skywalker", luke.Surname);

            await client.DeleteDatabaseAsync<Rebel>().ConfigureAwait(false);
        }
    }
}
