using CouchDB.Driver.Extensions;
using CouchDB.Driver.Types;
using CouchDB.Driver.UnitTests.Models;
using System;
using System.Linq;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Find_Selector_Conditions
    {
        CouchDatabase<Rebel> customers;

        public Find_Selector_Conditions()
        {
            var client = new CouchClient("http://localhost");
            customers = client.GetDatabase<Rebel>();
        }

        #region (In)Equality

        [Fact]
        public void InEquality_LessThan()
        {
            var json = customers.Where(c => c.Age < 19).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$lt"":19}}}", json);
        }
        [Fact]
        public void InEquality_LessThanOrEqual()
        {
            var json = customers.Where(c => c.Age <= 19).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$lte"":19}}}", json);
        }
        [Fact]
        public void InEquality_Equal()
        {
            var json = customers.Where(c => c.Age == 19).ToString();
            Assert.Equal(@"{""selector"":{""age"":19}}", json);
        }
        [Fact]
        public void InEquality_NotEqual()
        {
            var json = customers.Where(c => c.Age != 19).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$ne"":19}}}", json);
        }
        [Fact]
        public void InEquality_GreaterThanOrEqual()
        {
            var json = customers.Where(c => c.Age >= 19).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$gte"":19}}}", json);
        }
        [Fact]
        public void InEquality_GreaterThan()
        {
            var json = customers.Where(c => c.Age > 19).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$gt"":19}}}", json);
        }

        #endregion

        #region Object

        [Fact]
        public void Object_FieldExists()
        {
            var json = customers.Where(c => c.Age.FieldExists(true)).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$exists"":true}}}", json);
        }
        [Fact]
        public void Object_IsCouchType()
        {
            var json = customers.Where(c => c.Age.IsCouchType(CouchType.Number)).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$type"":""number""}}}", json);
        }

        #endregion

        #region Array

        [Fact]
        public void Array_In()
        {
            var json = customers.Where(c => c.Skills.In(new[] { "lightsaber", "force" })).ToString();
            Assert.Equal(@"{""selector"":{""skills"":{""$in"":[""lightsaber"",""force""]}}}", json);
        }
        [Fact]
        public void Array_NotIn()
        {
            var json = customers.Where(c => !c.Skills.In(new[] { "lightsaber", "force" })).ToString();
            Assert.Equal(@"{""selector"":{""skills"":{""$nin"":[""lightsaber"",""force""]}}}", json);
        }
        [Fact]
        public void Array_Size()
        {
            var json = customers.Where(c => c.Skills.Count == 2).ToString();
            Assert.Equal(@"{""selector"":{""skills"":{""$size"":2}}}", json);
        }

        #endregion

        #region Miscellaneous

        [Fact]
        public void Miscellaneous_Mod()
        {
            var json = customers.Where(c => c.Age % 2 == 0).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$mod"":[2,0]}}}", json);
        }
        [Fact]
        public void Miscellaneous_Regex()
        {
            var json = customers.Where(c => c.Name.IsMatch(@"^FN-[0-9]{4}$")).ToString();
            Assert.Equal(@"{""selector"":{""name"":{""$regex"":""^FN-[0-9]{4}$""}}}", json);
        }

        #endregion
    }
}
