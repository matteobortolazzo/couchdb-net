using CouchDB.Driver.UnitTests.Models;
using System;
using Xunit;
using CouchDB.Driver.Extensions;
using System.Linq;

namespace CouchDB.Driver.UnitTests
{
    public class Find_Selector_Combinations
    {
        CouchDatabase<Rebel> customers;

        public Find_Selector_Combinations()
        {
            var client = new CouchClient("http://localhost");
            customers = client.GetDatabase<Rebel>();
        }

        [Fact]
        public void And()
        {
            var json = customers.Where(c => c.Name == "Luke" && c.Surname == "Skywalker").ToString();
            Assert.Equal(@"{""selector"":{""$and"":[{""name"":""Luke""},{""surname"":""Skywalker""}]}}", json);
        }
        [Fact]
        public void Or()
        {
            var json = customers.Where(c => c.Name == "Luke" || c.Name == "Leia").ToString();
            Assert.Equal(@"{""selector"":{""$or"":[{""name"":""Luke""},{""name"":""Leia""}]}}", json);
        }
        [Fact]
        public void Not()
        {
            var json = customers.Where(c => !(c.Name == "Luke" && c.Surname == "Skywalker")).ToString();
            Assert.Equal(@"{""selector"":{""$not"":{""$and"":[{""name"":""Luke""},{""surname"":""Skywalker""}]}}}", json);
        }
        [Fact]
        public void Nor()
        {
            var json = customers.Where(c => !(c.Name == "Luke" || c.Name == "Leia")).ToString();
            Assert.Equal(@"{""selector"":{""$nor"":[{""name"":""Luke""},{""name"":""Leia""}]}}", json);
        }
        [Fact]
        public void All()
        {
            var json = customers.Where(c => c.Skills.Contains(new[] { "lightsaber", "force" })).ToString();
            Assert.Equal(@"{""selector"":{""skills"":{""$all"":[""lightsaber"",""force""]}}}", json);
        }
        [Fact]
        public void ElemMatch()
        {
            var json = customers.Where(c => c.Battles.Any(b => b.Planet == "Naboo")).ToString();
            Assert.Equal(@"{""selector"":{""battles"":{""$elemMatch"":{""planet"":""Naboo""}}}}", json);
        }
        [Fact]
        public void AllMatch()
        {
            var json = customers.Where(c => c.Battles.All(b => b.Planet == "Naboo")).ToString();
            Assert.Equal(@"{""selector"":{""battles"":{""$allMatch"":{""planet"":""Naboo""}}}}", json);
        }
    }
}
