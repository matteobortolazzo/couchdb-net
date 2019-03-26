using CouchDB.Driver.UnitTests.Models;
using System;
using System.Linq.Expressions;
using Xunit;

namespace CouchDB.Driver.UnitTests.Find
{
    public class Find_Selector
    {
        private readonly CouchDatabase<Rebel> rebels;

        public Find_Selector()
        {
            var client = new CouchClient("http://localhost:5984");
            rebels = client.GetDatabase<Rebel>();
        }

        [Fact]
        public void ToList_EmptySelector()
        {
            var json = rebels.ToString();
            Assert.Equal(@"{""selector"":{}}", json);
        }
        [Fact]
        public void ComplexQuery()
        {
            var json = rebels.Where(r => 
            r.Age == 19 && 
            (r.Name == "Luke" || r.Name == "Leia") && 
            r.Skills.Contains("force")).ToString();
            Assert.Equal(@"{""selector"":{""$and"":[{""age"":19},{""$or"":[{""name"":""Luke""},{""name"":""Leia""}]},{""skills"":{""$all"":[""force""]}}]}}", json);
        }
        [Fact]
        public void Variable_Const()
        {
            var age = 19;
            var json = rebels.Where(r => r.Age == age).ToString();
            Assert.Equal(@"{""selector"":{""age"":19}}", json);
        }
        [Fact]
        public void Variable_Object()
        {
            var luke = new Rebel { Age = 19 };
            var json = rebels.Where(r => r.Age == luke.Age).ToString();
            Assert.Equal(@"{""selector"":{""age"":19}}", json);
        }
        [Fact]
        public void ExpressionVariable_Const()
        {
            Expression<Func<Rebel, bool>> filter = r => r.Age == 19;
            var json = rebels.Where(filter).ToString();
            Assert.Equal(@"{""selector"":{""age"":19}}", json);
        }
        [Fact]
        public void ExpressionVariable_Object()
        {
            var luke = new Rebel { Age = 19 };
            Expression<Func<Rebel, bool>> filter = r => r.Age == luke.Age;
            var json = rebels.Where(filter).ToString();
            Assert.Equal(@"{""selector"":{""age"":19}}", json);
        }
        [Fact]
        public void Enum()
        {
            var json = rebels.Where(r => r.Species == Species.Human).ToString();
            Assert.Equal(@"{""selector"":{""species"":0}}", json);
        }
    }
}
