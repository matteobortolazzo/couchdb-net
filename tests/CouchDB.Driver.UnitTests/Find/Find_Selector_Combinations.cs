using CouchDB.Driver.UnitTests.Models;
using System;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Find_Selector_Combinations
    {
        CouchDatabase<Customer> customers;

        public Find_Selector_Combinations()
        {
            var client = new CouchClient("http://localhost");
            customers = client.GetDatabase<Customer>();
        }
    }
}
