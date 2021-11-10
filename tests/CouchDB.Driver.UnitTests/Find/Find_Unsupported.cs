using System;
using System.Linq;
using CouchDB.UnitTests.Models;
using Xunit;

namespace CouchDB.Driver.UnitTests.Find
{
    public class Find_Unsupported
    {
        private readonly ICouchDatabase<Rebel> _rebels;

        public Find_Unsupported()
        {
            var client = new CouchClient("http://localhost");
            _rebels = client.GetDatabase<Rebel>();
        }
        
        [Fact]
        public void ToList_WhereCount_Exception()
        {
            void CountQuery() => _rebels
                    .Where(u => u.Battles.Count > 0)
                    .ToString();

            Assert.Throws<NotSupportedException>(CountQuery);
        }
    }
}
