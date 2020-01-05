using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Attachments_Tests
    {
        private readonly CouchDatabase<Rebel> _rebels;

        public Attachments_Tests()
        {
            var client = new CouchClient("http://localhost");
            _rebels = client.GetDatabase<Rebel>();
        }

        [Fact]
        public void NewDocument_EmptyAttachmentsList()
        {
            var r = new Rebel { Name = "Luke" };
            Assert.Empty(r.Attachments);
        }

        [Fact]
        public void AddedAttachment_ShouldBeInList()
        {
            var r = new Rebel { Name = "Luke" };
            r.Attachments.Add(new FileInfo("luke.txt"), MediaTypeNames.Text.Plain);
            Assert.NotEmpty(r.Attachments);
        }

        [Fact]
        public void RemovedAttachment_ShouldBeNotInList()
        {
            var r = new Rebel { Name = "Luke" };
            r.Attachments.Add(new FileInfo("luke.txt"), MediaTypeNames.Text.Plain);
            r.Attachments.Delete("luke.txt");
            Assert.Empty(r.Attachments);

        }
    }
}
