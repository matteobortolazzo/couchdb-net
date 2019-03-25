using CouchDB.Driver.UnitTests.Models;
using System;
using Xunit;
using CouchDB.Driver.Extensions;
using System.Linq;

namespace CouchDB.Driver.UnitTests
{
    public class Find_Selector_Combinations
    {
        private readonly CouchDatabase<Rebel> rebels;

        public Find_Selector_Combinations()
        {
            var client = new CouchClient("http://localhost");
            rebels = client.GetDatabase<Rebel>();
        }

        [Fact]
        public void And()
        {
            var json = rebels.Where(r => r.Name == "Luke" && r.Surname == "Skywalker").ToString();
            Assert.Equal(@"{""selector"":{""$and"":[{""name"":""Luke""},{""surname"":""Skywalker""}]}}", json);
        }
        [Fact]
        public void Or()
        {
            var json = rebels.Where(r => r.Name == "Luke" || r.Name == "Leia").ToString();
            Assert.Equal(@"{""selector"":{""$or"":[{""name"":""Luke""},{""name"":""Leia""}]}}", json);
        }
        [Fact]
        public void Not()
        {
            var json = rebels.Where(r => !(r.Name == "Luke" && r.Surname == "Skywalker")).ToString();
            Assert.Equal(@"{""selector"":{""$not"":{""$and"":[{""name"":""Luke""},{""surname"":""Skywalker""}]}}}", json);
        }
        [Fact]
        public void Nor()
        {
            var json = rebels.Where(r => !(r.Name == "Luke" || r.Name == "Leia")).ToString();
            Assert.Equal(@"{""selector"":{""$nor"":[{""name"":""Luke""},{""name"":""Leia""}]}}", json);
        }
        [Fact]
        public void All_Single()
        {
            var json = rebels.Where(r => r.Skills.Contains("lightsaber")).ToString();
            Assert.Equal(@"{""selector"":{""skills"":{""$all"":[""lightsaber""]}}}", json);
        }
        [Fact]
        public void All_Array()
        {
            var json = rebels.Where(r => r.Skills.Contains(new[] { "lightsaber", "force" })).ToString();
            Assert.Equal(@"{""selector"":{""skills"":{""$all"":[""lightsaber"",""force""]}}}", json);
        }
        [Fact]
        public void ElemMatch()
        {
            var json = rebels.Where(r => r.Battles.Any(b => b.Planet == "Naboo")).ToString();
            Assert.Equal(@"{""selector"":{""battles"":{""$elemMatch"":{""planet"":""Naboo""}}}}", json);
        }
        [Fact]
        public void AllMatch()
        {
            var json = rebels.Where(r => r.Battles.All(b => b.Planet == "Naboo")).ToString();
            Assert.Equal(@"{""selector"":{""battles"":{""$allMatch"":{""planet"":""Naboo""}}}}", json);
        }
    }
}
