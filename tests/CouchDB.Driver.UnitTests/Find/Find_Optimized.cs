using System;
using System.Collections.Generic;
using System.Linq;
using CouchDB.UnitTests.Models;
using Flurl.Http.Testing;
using Xunit;

namespace CouchDB.Driver.UnitTests.Find
{
    public class Find_Optimized
    {
        private const string _databaseName = "allrebels";
        private readonly ICouchDatabase<Rebel> _rebels;
        private readonly object _response;

        public Find_Optimized()
        {
            var client = new CouchClient("http://localhost");
            _rebels = client.GetDatabase<Rebel>(_databaseName);

            var mainRebel = new Rebel
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Luke",
                Age = 19,
                Skills = new List<string> { "Force" }
            };
            var rebelsList = new List<Rebel>
            {
                mainRebel
            };
            _response = new
            {
                Docs = rebelsList
            };
        }

        [Fact]
        public void FirstOrDefault()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            _rebels.FirstOrDefault();
            Assert.Equal(@"{""limit"":1,""selector"":{}}", httpTest.CallLog[0].RequestBody);
        }

        [Fact]
        public void LastOrDefault()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            _rebels.LastOrDefault();
            Assert.Equal(@"{""selector"":{}}", httpTest.CallLog[0].RequestBody);
        }

        [Fact]
        public void FirstOrDefault_Predicate()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            _rebels.FirstOrDefault(r => r.Age == 19);
            Assert.Equal(@"{""selector"":{""age"":19},""limit"":1}", httpTest.CallLog[0].RequestBody);
        }

        [Fact]
        public void LastOrDefault_Predicate()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            _rebels.LastOrDefault(r => r.Age == 19);
            Assert.Equal(@"{""selector"":{""age"":19}}", httpTest.CallLog[0].RequestBody);
        }

        [Fact]
        public void FirstOrDefault_Predicate_Where()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            _rebels.Where(c => c.Name == "Luke").FirstOrDefault(r => r.Age == 19);
            Assert.Equal(@"{""selector"":{""$and"":[{""name"":""Luke""},{""age"":19}]},""limit"":1}", httpTest.CallLog[0].RequestBody);
        }

        [Fact]
        public void LastOrDefault_Predicate_Where()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            _rebels.Where(c => c.Name == "Luke").LastOrDefault(r => r.Age == 19);
            Assert.Equal(@"{""selector"":{""$and"":[{""name"":""Luke""},{""age"":19}]}}", httpTest.CallLog[0].RequestBody);
        }
    }
}
