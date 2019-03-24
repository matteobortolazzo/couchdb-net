using CouchDB.Driver.UnitTests.Models;
using System;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Find_Selector
    {
        CouchDatabase<Customer> customers;

        public Find_Selector()
        {
            var client = new CouchClient("http://localhost");
            customers = client.GetDatabase<Customer>();
        }

        [Fact]
        public void ToList_EmptySelector()
        {
            var json = customers.ToString();
            Assert.Equal("{\"selector\":{}}", json);
        }
    }
}
