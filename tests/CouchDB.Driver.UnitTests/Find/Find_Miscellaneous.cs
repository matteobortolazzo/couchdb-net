using CouchDB.Driver.UnitTests.Models;
using System;
using System.Linq;
using Xunit;
using CouchDB.Driver.Extensions;

namespace CouchDB.Driver.UnitTests.Find
{
    public class Find_Miscellaneous
    {
        private readonly CouchDatabase<Rebel> _rebels;

        public Find_Miscellaneous()
        {
            var client = new CouchClient("http://localhost:5984");
            _rebels = client.GetDatabase<Rebel>();
        }

        [Fact]
        public void Limit()
        {
            var json = _rebels.Take(20).ToString();
            Assert.Equal(@"{""limit"":20,""selector"":{}}", json);
        }
        [Fact]
        public void Skip()
        {
            var json = _rebels.Skip(20).ToString();
            Assert.Equal(@"{""skip"":20,""selector"":{}}", json);
        }
        [Fact]
        public void Fields()
        {
            var json = _rebels.Select(r => new {
                r.Name,
                r.Age
            }).ToString();
            Assert.Equal(@"{""fields"":[""name"",""age""],""selector"":{}}", json);
        }
        [Fact]
        public void Index_Parlial()
        {
            var json = _rebels.UseIndex("design_document").ToString();
            Assert.Equal(@"{""use_index"":""design_document"",""selector"":{}}", json);
        }
        [Fact]
        public void Index_Complete()
        {
            var json = _rebels.UseIndex("design_document", "index_name").ToString();
            Assert.Equal(@"{""use_index"":[""design_document"",""index_name""],""selector"":{}}", json);
        }
        [Fact]
        public void Index_TooMuchArguments()
        {
            var exception = Record.Exception(() =>
            {
                var json = _rebels.UseIndex("arg1", "arg2", "arg3").ToString();
            });
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
        }
        [Fact]
        public void Quorum()
        {
            var json = _rebels.WithReadQuorum(20).ToString();
            Assert.Equal(@"{""r"":20,""selector"":{}}", json);
        }
        [Fact]
        public void Bookmark()
        {
            var json = _rebels.UseBookmark("g1AAAABweJzLY...").ToString();
            Assert.Equal(@"{""bookmark"":""g1AAAABweJzLY..."",""selector"":{}}", json);
        }
        [Fact]
        public void Update()
        {
            var json = _rebels.WithoutIndexUpdate().ToString();
            Assert.Equal(@"{""update"":false,""selector"":{}}", json);
        }
        [Fact]
        public void Stable()
        {
            var json = _rebels.FromStable().ToString();
            Assert.Equal(@"{""stable"":true,""selector"":{}}", json);
        }
        [Fact]
        public void Combinations()
        {
            var json = _rebels
                .Where(r => 
                    r.Surname == "Skywalker" && 
                    (
                        r.Battles.All(b => b.Planet == "Naboo") ||
                        r.Battles.Any(b => b.Planet == "Death Star")
                    )
                )
                .OrderByDescending(r => r.Name)
                .ThenByDescending(r => r.Age)
                .Skip(1)
                .Take(2)
                .WithReadQuorum(2)
                .UseBookmark("g1AAAABweJzLY...")
                .WithReadQuorum(150)
                .WithoutIndexUpdate()
                .FromStable()
                .Select(r => new {
                    r.Name,
                    r.Age,
                    r.Species
                }).ToString();
            Assert.Equal(@"{""selector"":{""$and"":[{""surname"":""Skywalker""},{""$or"":[{""battles"":{""$allMatch"":{""planet"":""Naboo""}}},{""battles"":{""$elemMatch"":{""planet"":""Death Star""}}}]}]},""sort"":[{""name"":""desc""},{""age"":""desc""}],""skip"":1,""limit"":2,""r"":2,""bookmark"":""g1AAAABweJzLY..."",""r"":150,""update"":false,""stable"":true,""fields"":[""name"",""age"",""species""]}", json);
        }
    }
}
