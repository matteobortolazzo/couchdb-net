using System;
using System.Collections.Generic;
using CouchDB.UnitTests.Models;
using System.Linq;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Query.Extensions;
using Flurl.Http.Testing;
using Xunit;

namespace CouchDB.Driver.UnitTests.Find
{
    public class Find_Discriminator
    {
        private const string _databaseName = "allrebels";
        private readonly ICouchDatabase<Rebel> _rebels;
        private readonly ICouchDatabase<SimpleRebel> _simpleRebels;
        private readonly object _response;

        public Find_Discriminator()
        {
            var client = new CouchClient("http://localhost");
            _rebels = client.GetDatabase<Rebel>(_databaseName, nameof(Rebel));
            _simpleRebels = client.GetDatabase<SimpleRebel>(_databaseName, nameof(SimpleRebel));

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
        public void Discriminator_WithoutFilter()
        {
            var json1 = _rebels.ToString();
            var json2 = _simpleRebels.ToString();
            Assert.Equal(@"{""selector"":{""split_discriminator"":""Rebel""}}", json1);
            Assert.Equal(@"{""selector"":{""split_discriminator"":""SimpleRebel""}}", json2);
        }

        [Fact]
        public void Discriminator_WithFilter()
        {
            var json1 = _rebels.Where(c => c.Age == 19).ToString();
            var json2 = _simpleRebels.Where(c => c.Age == 19).ToString();
            Assert.Equal(@"{""selector"":{""$and"":[{""age"":19},{""split_discriminator"":""Rebel""}]}}", json1);
            Assert.Equal(@"{""selector"":{""$and"":[{""age"":19},{""split_discriminator"":""SimpleRebel""}]}}", json2);
        }

        [Fact]
        public void Discriminator_FirstOrDefault()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            _rebels.FirstOrDefault();
            _simpleRebels.FirstOrDefault();
            Assert.Equal(@"{""selector"":{""split_discriminator"":""Rebel""},""limit"":1}", httpTest.CallLog[0].RequestBody);
            Assert.Equal(@"{""selector"":{""split_discriminator"":""SimpleRebel""},""limit"":1}", httpTest.CallLog[1].RequestBody);
        }

        [Fact]
        public void Discriminator_FirstOrDefault_WithExpression()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            _rebels.FirstOrDefault(c => c.Age == 19);
            _simpleRebels.FirstOrDefault(c => c.Age == 19);
            Assert.Equal(@"{""selector"":{""$and"":[{""split_discriminator"":""Rebel""},{""age"":19}]},""limit"":1}", httpTest.CallLog[0].RequestBody);
            Assert.Equal(@"{""selector"":{""$and"":[{""split_discriminator"":""SimpleRebel""},{""age"":19}]},""limit"":1}", httpTest.CallLog[1].RequestBody);
        }
        
        [Fact]
        public void Discriminator_FirstOrDefault_WithWhere()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var query = _rebels.Where(r => r.Id == "1").Select(r => r.Id, r => r.Rev);
            query.FirstOrDefault();
            query.FirstOrDefault(r => r.Age == 19);
            query.LastOrDefault();
            query.LastOrDefaultAsync(r => r.Age == 19);
            Assert.Equal(@"{""fields"":[""_id"",""_rev""],""selector"":{""$and"":[{""_id"":""1""},{""split_discriminator"":""Rebel""}]},""limit"":1}", httpTest.CallLog[0].RequestBody);
            Assert.Equal(@"{""fields"":[""_id"",""_rev""],""selector"":{""$and"":[{""_id"":""1""},{""split_discriminator"":""Rebel""},{""age"":19}]},""limit"":1}", httpTest.CallLog[1].RequestBody);
        }
        
        [Fact]
        public void Discriminator_LastOrDefault_WithWhere()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var query = _rebels.Where(r => r.Id == "1").Select(r => r.Id, r => r.Rev);
            query.LastOrDefault();
            query.LastOrDefaultAsync(r => r.Age == 19);
            Assert.Equal(@"{""fields"":[""_id"",""_rev""],""selector"":{""$and"":[{""_id"":""1""},{""split_discriminator"":""Rebel""}]}}", httpTest.CallLog[0].RequestBody);
            Assert.Equal(@"{""fields"":[""_id"",""_rev""],""selector"":{""$and"":[{""_id"":""1""},{""split_discriminator"":""Rebel""},{""age"":19}]}}", httpTest.CallLog[1].RequestBody);
        }
    }
}
