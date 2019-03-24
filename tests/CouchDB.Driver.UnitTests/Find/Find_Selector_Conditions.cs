using CouchDB.Driver.Extensions;
using CouchDB.Driver.Types;
using CouchDB.Driver.UnitTests.Models;
using System;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class Find_Selector_Conditions
    {
        CouchDatabase<Customer> customers;

        public Find_Selector_Conditions()
        {
            var client = new CouchClient("http://localhost");
            customers = client.GetDatabase<Customer>();
        }

        #region (In)Equality

        [Fact]
        public void InEquality_LessThan()
        {
            var json = customers.Where(c => c.Age < 24).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$lt"":24}}}", json);
        }
        [Fact]
        public void InEquality_LessThanOrEqual()
        {
            var json = customers.Where(c => c.Age <= 24).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$lte"":24}}}", json);
        }
        [Fact]
        public void InEquality_Equal()
        {
            var json = customers.Where(c => c.Age == 24).ToString();
            Assert.Equal(@"{""selector"":{""age"":24}}", json);
        }
        [Fact]
        public void InEquality_NotEqual()
        {
            var json = customers.Where(c => c.Age != 24).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$ne"":24}}}", json);
        }
        [Fact]
        public void InEquality_GreaterThanOrEqual()
        {
            var json = customers.Where(c => c.Age >= 24).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$gte"":24}}}", json);
        }
        [Fact]
        public void InEquality_GreaterThan()
        {
            var json = customers.Where(c => c.Age > 24).ToString();
            Assert.Equal(@"{""selector"":{""age"":{""$gt"":24}}}", json);
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
        public void Array_Contains()
        {
            var json = customers.Where(c => c.Hobbies.Contains("sport")).ToString();
            Assert.Equal(@"{""selector"":{""hobbies"":{""$in"":[""sport""]}}}", json);
        }
        [Fact]
        public void Array_In()
        {
            var json = customers.Where(c => c.Hobbies.In(new[] { "sport", "coding" })).ToString();
            Assert.Equal(@"{""selector"":{""hobbies"":{""$in"":[""sport"",""coding""]}}}", json);
        }
        [Fact]
        public void Array_NotContains()
        {
            var json = customers.Where(c => !c.Hobbies.Contains("sport")).ToString();
            Assert.Equal(@"{""selector"":{""hobbies"":{""$nin"":[""sport""]}}}", json);
        }
        [Fact]
        public void Array_NotIn()
        {
            var json = customers.Where(c => c.Hobbies.NotIn(new[] { "sport", "coding" })).ToString();
            Assert.Equal(@"{""selector"":{""hobbies"":{""$nin"":[""sport"",""coding""]}}}", json);
        }
        [Fact]
        public void Array_Size()
        {
            var json = customers.Where(c => c.Hobbies.Count == 2).ToString();
            Assert.Equal(@"{""selector"":{""hobbies"":{""$size"":2}}}", json);
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
            var json = customers.Where(c => c.Email.IsMatch(@"^.+\@.+\.[a-z]+$")).ToString();
            Assert.Equal(@"{""selector"":{""email"":{""$regex"":""^.+\@.+\.[a-z]+$""}}}", json);
        }

        #endregion
    }
}
