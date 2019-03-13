using CouchDB.Client.Query.Selector;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq.Expressions;

namespace CouchDB.Client.Tests
{
    [TestClass]
    public class SelectorTests
    {
        class MyDocument : CouchEntity
        {
            public int MyInt { get; set; }

            public MyDocument MyDoc { get; set; }
        }

        [TestMethod]
        public void EqualsConstantTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt == 1;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":1}");
        }

        [TestMethod]
        public void GreaterThanConstantTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt > 1;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":{\"$gt\":1}}");
        }

        [TestMethod]
        public void GreaterThanOrEqualConstantTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt >= 1;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":{\"$gte\":1}}");
        }

        [TestMethod]
        public void LessThanConstantTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt < 1;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":{\"$lt\":1}}");
        }

        [TestMethod]
        public void LessThanOrEqualConstantTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt <= 1;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":{\"$lte\":1}}");
        }

        [TestMethod]
        public void NestedEqualsConstantTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyDoc.MyInt == 1;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyDoc.MyInt\":1}");
        }

        [TestMethod]
        public void NestedGreaterThanConstantTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyDoc.MyInt > 1;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyDoc.MyInt\":{\"$gt\":1}}");
        }

        [TestMethod]
        public void NestedGreaterThanOrEqualConstantTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyDoc.MyInt >= 1;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyDoc.MyInt\":{\"$gte\":1}}");
        }

        [TestMethod]
        public void NestedLessThanConstantTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyDoc.MyInt < 1;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyDoc.MyInt\":{\"$lt\":1}}");
        }

        [TestMethod]
        public void NestedLessThanOrEqualConstantTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyDoc.MyInt <= 1;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyDoc.MyInt\":{\"$lte\":1}}");
        }

        int one = 1;

        [TestMethod]
        public void EqualsVariableTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt == one;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":1}");
        }

        [TestMethod]
        public void GreaterThanVariableTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt > one;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":{\"$gt\":1}}");
        }

        [TestMethod]
        public void GreaterThanOrEqualVariableTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt >= one;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":{\"$gte\":1}}");
        }

        [TestMethod]
        public void LessThanVariableTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt < one;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":{\"$lt\":1}}");
        }

        [TestMethod]
        public void LessThanOrEqualVariableTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt <= one;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":{\"$lte\":1}}");
        }

        MyDocument comparisonDoc = new MyDocument()
        {
            MyInt = 1
        };

        [TestMethod]
        public void EqualsObjectPropertyTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt == comparisonDoc.MyInt;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":1}");
        }

        [TestMethod]
        public void GreaterThanObjectPropertyTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt > comparisonDoc.MyInt;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":{\"$gt\":1}}");
        }

        [TestMethod]
        public void GreaterThanOrEqualObjectPropertyTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt >= comparisonDoc.MyInt;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":{\"$gte\":1}}");
        }

        [TestMethod]
        public void LessThanObjectPropertyTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt < comparisonDoc.MyInt;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":{\"$lt\":1}}");
        }

        [TestMethod]
        public void LessThanOrEqualObjectPropertyTest()
        {
            Expression<Func<MyDocument, bool>> query = t => t.MyInt <= comparisonDoc.MyInt;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":{\"$lte\":1}}");
        }

        [TestMethod]
        public void LessThanOrEqualObjectPropertyTest_VeryNested()
        {
            comparisonDoc = new MyDocument() { MyDoc = new MyDocument() { MyDoc = comparisonDoc } };
            Expression<Func<MyDocument, bool>> query = t => t.MyInt <= comparisonDoc.MyDoc.MyDoc.MyInt;

            var selector = SelectorObjectBuilder.Serialize<MyDocument>(query);

            var jsonRequest = JsonConvert.SerializeObject(selector);

            jsonRequest.Should().Be("{\"MyInt\":{\"$lte\":1}}");
        }
    }
}
