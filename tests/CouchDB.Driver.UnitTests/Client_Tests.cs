using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Client_Tests
    {
        [Fact]
        public void Creation_Valid()
        {
            using (var client = new CouchClient("http://localhost:5984"))
            {
                Assert.Equal("http://localhost:5984", client.ConnectionString);
            }
        }
        [Fact]
        public void Creation_NullConnectionString()
        {
            var exception = Record.Exception(() =>
            {
                new CouchClient(null);
            });
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }
        [Fact]
        public void Creation_BasicAuthentication()
        {
            using (var client = new CouchClient("http://localhost:5984", s => 
                s.ConfigureBasicAuthentication("root", "relax")))
            {
                Assert.Equal("http://localhost:5984", client.ConnectionString);
            }
        }
        [Fact]
        public void Creation_CookieAuthentication()
        {
            using (var client = new CouchClient("http://localhost:5984", s =>
                s.ConfigureCookieAuthentication("root", "relax")))
            {
                Assert.Equal("http://localhost:5984", client.ConnectionString);
            }
        }
    }
}
