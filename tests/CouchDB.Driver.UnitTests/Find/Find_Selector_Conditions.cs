using CouchDB.Driver.Extensions;
using CouchDB.Driver.Types;
using CouchDB.UnitTests.Models;
using System;
using System.Linq;
using CouchDB.Driver.Query.Extensions;
using Xunit;
using System.Text.RegularExpressions;

namespace CouchDB.Driver.UnitTests.Find
{
    public class Find_Selector_Conditions
    {
        private readonly ICouchDatabase<Rebel> _rebels;

        public Find_Selector_Conditions()
        {
            var client = new CouchClient("http://localhost");
            _rebels = client.GetDatabase<Rebel>();
        }

        #region (In)Equality

        [Fact]
        public void InEquality_LessThan()
        {
            var json = _rebels.Where(r => r.Age < 19).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$lt"":19}}}", json);
        }

        [Fact]
        public void InEquality_LessThanOrEqual()
        {
            var json = _rebels.Where(r => r.Age <= 19).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$lte"":19}}}", json);
        }

        [Fact]
        public void InEquality_Equal()
        {
            var json = _rebels.Where(r => r.Age == 19).ToString();
            Assert.Equal(@"{""selector"":{""age"":19}}", json);
        }

        [Fact]
        public void InEquality_NotEqual()
        {
            var json = _rebels.Where(r => r.Age != 19).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$ne"":19}}}", json);
        }

        [Fact]
        public void InEquality_GreaterThanOrEqual()
        {
            var json = _rebels.Where(c => c.Age >= 19).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$gte"":19}}}", json);
        }

        [Fact]
        public void InEquality_GreaterThan()
        {
            var json = _rebels.Where(r => r.Age > 19).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$gt"":19}}}", json);
        }

        #endregion

        #region Object

        [Fact]
        public void Object_FieldExists()
        {
            var json = _rebels.Where(r => r.FieldExists("age")).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$exists"":true}}}", json);
        }

        [Fact]
        public void Object_IsCouchType()
        {
            var json = _rebels.Where(r => r.Age.IsCouchType(CouchType.CNumber)).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$type"":""number""}}}", json);
        }

        #endregion

        #region Array

        [Fact]
        public void Array_In()
        {
            var json = _rebels.Where(r => r.Age.In(new[] { 20, 30 })).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$in"":[20,30]}}}", json);
        }

        [Fact]
        public void Array_In_Guid()
        {
            var json = _rebels.Where(r => r.Guid.In(new[] { Guid.Parse("00000000-0000-0000-0000-000000000000"), Guid.Parse("11111111-1111-1111-1111-111111111111") })).ToString();
            Assert.Equal(@"{""selector"":{""guid"":{""$in"":[""00000000-0000-0000-0000-000000000000"",""11111111-1111-1111-1111-111111111111""]}}}", json);
        }

        [Fact]
        public void Array_InSingleItem()
        {
            var json = _rebels.Where(r => r.Age.In(new[] { 20 })).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$in"":[20]}}}", json);
        }

        [Fact]
        public void Array_InEnum()
        {
            var json = _rebels.Where(r => r.Species.In(new[] { Species.Human, Species.Droid })).ToString();
            Assert.Equal(@"{""selector"":{""species"":{""$in"":[0,1]}}}", json);
        }

        [Fact]
        public void Array_NotIn()
        {
            var json = _rebels.Where(r => !r.Age.In(new[] { 20, 30 })).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$nin"":[20,30]}}}", json);
        }

        [Fact]
        public void Array_Size()
        {
            var json = _rebels.Where(r => r.Skills.Count == 2).ToString();
            Assert.Equal(@"{""selector"":{""skills"":{""$size"":2}}}", json);
        }

        #endregion

        #region Miscellaneous

        [Fact]
        public void Miscellaneous_Mod()
        {
            var json = _rebels.Where(r => r.Age % 2 == 0).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$mod"":[2,0]}}}", json);
        }

        [Fact]
        public void Miscellaneous_Regex()
        {
            var json = _rebels.Where(r => r.Name.IsMatch(@"^FN-[0-9]{4}$")).ToString();
            Assert.Equal(@"{""selector"":{""name"":{""$regex"":""^FN-[0-9]{4}$""}}}", json);
        }

        [Fact]
        public void Miscellaneous_RegexMultiLine()
        {
            var json = _rebels.Where(r => r.Skills.Any(s => s.IsMatch($"(?i)sab") || s.IsMatch($"(?)orce"))).ToString();
            Assert.Equal(@"{""selector"":{""skills"":{""$elemMatch"":{""$or"":[{""$regex"":""(?i)sab""},{""$regex"":""(?)orce""}]}}}}", json);
        }

        #endregion
    }
}
