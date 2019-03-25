using CouchDB.Driver.UnitTests.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;

namespace CouchDB.Driver.UnitTests.Find
{
    public class Find_Sort
    {
        private readonly CouchDatabase<Rebel> rebels;

        public Find_Sort()
        {
            var client = new CouchClient("http://localhost");
            rebels = client.GetDatabase<Rebel>();
        }

        [Fact]
        public void SortAsc()
        {
            var json = rebels.OrderBy(c => c.Name).ToString();
            Assert.Equal(@"{""sort"":[""name""],""selector"":{}}", json);
        }
        [Fact]
        public void SortDesc()
        {
            var json = rebels.OrderByDescending(c => c.Name).ToString();
            Assert.Equal(@"{""sort"":[{""name"":""desc""}],""selector"":{}}", json);
        }
        [Fact]
        public void SortAscMultiple()
        {
            var json = rebels.OrderBy(c => c.Name).ThenBy(c => c.Age).ToString();
            Assert.Equal(@"{""sort"":[""name"",""age""],""selector"":{}}", json);
        }
        [Fact]
        public void SortDescMultiple()
        {
            var json = rebels.OrderByDescending(c => c.Name).ThenByDescending(c => c.Age).ToString();
            Assert.Equal(@"{""sort"":[{""name"":""desc""},{""age"":""desc""}],""selector"":{}}", json);
        }
        [Fact]
        public void SortAscMultiple_DifferentDirections()
        {
            var exception = Record.Exception(() =>
            {
                rebels.OrderBy(c => c.Name).ThenByDescending(c => c.Age).ToString();
            });
            Assert.NotNull(exception);
            Assert.IsType<InvalidOperationException>(exception);
        }
        [Fact]
        public void SortDescMultiple_DifferentDirections()
        {
            var exception = Record.Exception(() =>
            {
                rebels.OrderByDescending(c => c.Name).ThenBy(c => c.Age).ToString();
            });
            Assert.NotNull(exception);
            Assert.IsType<InvalidOperationException>(exception);
        }
    }
}
