using CouchDB.Driver.UnitTests.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CouchDB.Driver.UnitTests.Find
{
    public class Find_FilteringFields
    {
        private readonly CouchDatabase<Rebel> rebels;

        public Find_FilteringFields()
        {
            var client = new CouchClient("http://localhost");
            rebels = client.GetDatabase<Rebel>();
        }

        [Fact]
        public void And()
        {
            var json = rebels.Select(r => new {
                r.Name,
                r.Age
            }).ToString();
            Assert.Equal(@"{""fields"":[""name"",""age""],""selector"":{}}", json);
        }
    }
}
