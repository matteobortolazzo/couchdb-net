using CouchDB.Driver.UnitTests.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace CouchDB.Driver.UnitTests.Find
{
    public class Find_Selector
    {
        private readonly CouchDatabase<Rebel> rebels;

        public Find_Selector()
        {
            var client = new CouchClient("http://localhost");
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
        [Fact]
        public void GuidQuery()
        {
            var guidString = "83c79283-f634-41e3-8aab-674bdbae3413";
            var guid = Guid.Parse(guidString);
            var json = rebels.Where(r => r.Guid == guid).ToString();
            Assert.Equal(@"{""selector"":{""guid"":""83c79283-f634-41e3-8aab-674bdbae3413""}}", json);
        }
        [Fact]
        public void Variable_Bool_True()
        {
            var json = rebels.Where(r => r.IsJedi).OrderBy(r => r.IsJedi).ToString();
            Assert.Equal(@"{""selector"":{""isJedi"":true},""sort"":[""isJedi""]}", json);
        }
        [Fact]
        public void Variable_Bool_False()
        {
            var json = rebels.Where(r => !r.IsJedi).OrderBy(r => r.IsJedi).ToString();
            Assert.Equal(@"{""selector"":{""isJedi"":false},""sort"":[""isJedi""]}", json);
        }
        [Fact]
        public void Variable_Bool_ExplicitTrue()
        {
            var json = rebels.Where(r => r.IsJedi == true).OrderBy(r => r.IsJedi).ToString();
            Assert.Equal(@"{""selector"":{""isJedi"":true},""sort"":[""isJedi""]}", json);
        }
        [Fact]
        public void Variable_Bool_ExplicitFalse()
        {
            var json = rebels.Where(r => r.IsJedi == false).OrderBy(r => r.IsJedi).ToString();
            Assert.Equal(@"{""selector"":{""isJedi"":false},""sort"":[""isJedi""]}", json);
        }
        [Fact]
        public void Variable_Bool_ExplicitNotTrue()
        {
            var json = rebels.Where(r => r.IsJedi != true).OrderBy(r => r.IsJedi).ToString();
            Assert.Equal(@"{""selector"":{""isJedi"":{""$ne"":true}},""sort"":[""isJedi""]}", json);
        }
        [Fact]
        public void Variable_Bool_ExplicitNotFalse()
        {
            var json = rebels.Where(r => r.IsJedi != false).OrderBy(r => r.IsJedi).ToString();
            Assert.Equal(@"{""selector"":{""isJedi"":{""$ne"":false}},""sort"":[""isJedi""]}", json);
        }
    }
}
