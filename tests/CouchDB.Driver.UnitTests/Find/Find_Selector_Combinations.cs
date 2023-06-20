using CouchDB.UnitTests.Models;
using System;
using Xunit;
using System.Linq;
using CouchDB.Driver.Query.Extensions;
using Flurl.Http.Testing;

namespace CouchDB.Driver.UnitTests.Find
{
    public class Find_Selector_Combinations
    {
        private readonly ICouchDatabase<Rebel> _rebels;

        public Find_Selector_Combinations()
        {
            var client = new CouchClient("http://localhost");
            _rebels = client.GetDatabase<Rebel>();
        }

        [Fact]
        public void And_HttpCall()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                docs = new[]
                {
                    new Rebel()
                }
            });
            var surname = "Skywalker";
            var result = _rebels.Where(r => r.Name == "Luke" && r.Surname == surname).ToList();
            Assert.NotEmpty(result);

            var actualBody = httpTest.CallLog[0].RequestBody;
            var expectedBody = @"{""selector"":{""$and"":[{""name"":""Luke""},{""surname"":""Skywalker""}]}}";
            Assert.Equal(expectedBody, actualBody);
        }

        [Fact]
        public void And_WithVariable()
        {
            var surname = "Skywalker";
            var json = _rebels.Where(r => r.Name == "Luke" && r.Surname == surname).ToString();
            Assert.Equal(@"{""selector"":{""$and"":[{""name"":""Luke""},{""surname"":""Skywalker""}]}}", json);
        }

        [Fact]
        public void And()
        {
            var json = _rebels.Where(r => r.Name == "Luke" && r.Surname == "Skywalker").ToString();
            Assert.Equal(@"{""selector"":{""$and"":[{""name"":""Luke""},{""surname"":""Skywalker""}]}}", json);
        }

        [Fact]
        public void Or()
        {
            var json = _rebels.Where(r => r.Name == "Luke" || r.Name == "Leia").ToString();
            Assert.Equal(@"{""selector"":{""$or"":[{""name"":""Luke""},{""name"":""Leia""}]}}", json);
        }

        [Fact]
        public void Not()
        {
            var json = _rebels.Where(r => !(r.Name == "Luke" && r.Surname == "Skywalker")).ToString();
            Assert.Equal(@"{""selector"":{""$not"":{""$and"":[{""name"":""Luke""},{""surname"":""Skywalker""}]}}}",
                json);
        }

        [Fact]
        public void Nor()
        {
            var json = _rebels.Where(r => !(r.Name == "Luke" || r.Name == "Leia")).ToString();
            Assert.Equal(@"{""selector"":{""$nor"":[{""name"":""Luke""},{""name"":""Leia""}]}}", json);
        }

        [Fact]
        public void All_Single()
        {
            var json = _rebels.Where(r => r.Skills.Contains("lightsaber")).ToString();
            Assert.Equal(@"{""selector"":{""skills"":{""$all"":[""lightsaber""]}}}", json);
        }

        [Fact]
        public void All_Array()
        {
            var json = _rebels.Where(r => r.Skills.Contains(new[] { "lightsaber", "force" })).ToString();
            Assert.Equal(@"{""selector"":{""skills"":{""$all"":[""lightsaber"",""force""]}}}", json);
        }

        [Fact]
        public void ElemMatch()
        {
            var json = _rebels.Where(r => r.Battles.Any(b => b.Planet == "Naboo")).ToString();
            Assert.Equal(@"{""selector"":{""battles"":{""$elemMatch"":{""planet"":""Naboo""}}}}", json);
        }

        [Fact]
        public void ElemMatchImplicitBool()
        {
            var json = _rebels.Where(r => r.Battles.Any(b => b.DidWin)).ToString();
            Assert.Equal(@"{""selector"":{""battles"":{""$elemMatch"":{""didWin"":true}}}}", json);
        }

        [Fact]
        public void ElemMatchBoolExplicit()
        {
            var json = _rebels.Where(r => r.Battles.Any(b => b.DidWin == true)).ToString();
            Assert.Equal(@"{""selector"":{""battles"":{""$elemMatch"":{""didWin"":true}}}}", json);
        }

        [Fact]
        public void ElemMatchNested()
        {
            var json = _rebels.Where(r => r.Battles.Any(b => b.Vehicles.Any(v => v.CanFly == true))).ToString();
            Assert.Equal(
                @"{""selector"":{""battles"":{""$elemMatch"":{""vehicles"":{""$elemMatch"":{""canFly"":true}}}}}}",
                json);
        }

        [Fact]
        public void ElemMatchNestedImplicitBool()
        {
            var json = _rebels.Where(r => r.Battles.Any(b => b.Vehicles.Any(v => v.CanFly))).ToString();
            Assert.Equal(
                @"{""selector"":{""battles"":{""$elemMatch"":{""vehicles"":{""$elemMatch"":{""canFly"":true}}}}}}",
                json);
        }
        
        [Fact]
        public void ElemMatch_In()
        {
            var search = new[] { "battle" };
            var json = _rebels.Where(r => r.Skills.Any(s => s.In(search))).ToString();
            Assert.Equal(@"{""selector"":{""skills"":{""$elemMatch"":{""$in"":[""battle""]}}}}", json);
        }

        [Fact]
        public void AllMatch()
        {
            var json = _rebels.Where(r => r.Battles.All(b => b.Planet == "Naboo")).ToString();
            Assert.Equal(@"{""selector"":{""battles"":{""$allMatch"":{""planet"":""Naboo""}}}}", json);
        }

        [Fact]
        public void AllMatchImplicitBool()
        {
            var json = _rebels.Where(r => r.Battles.All(b => b.DidWin)).ToString();
            Assert.Equal(@"{""selector"":{""battles"":{""$allMatch"":{""didWin"":true}}}}", json);
        }
    }
}