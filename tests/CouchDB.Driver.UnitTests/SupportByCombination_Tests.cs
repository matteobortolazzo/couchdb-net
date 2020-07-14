using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class SupportByCombination_Tests
    {
        private readonly ICouchDatabase<Rebel> _rebels;
        private readonly Rebel _mainRebel;
        private readonly List<Rebel> _rebelsList;
        private readonly object _response;

        public SupportByCombination_Tests()
        {
            var client = new CouchClient("http://localhost");
            _rebels = client.GetDatabase<Rebel>();
            _mainRebel = new Rebel
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Luke",
                Age = 19,
                Skills = new List<string> { "Force" }
            };
            _rebelsList = new List<Rebel>
                {
                    _mainRebel
                };
            _response = new
            {
                Docs = _rebelsList
            };
        }

        [Fact]
        public void Max()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var result = _rebels.Max(r => r.Age);
            Assert.Equal(_mainRebel.Age, result);
        }

        [Fact]
        public void Min()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var result = _rebels.Min(r => r.Age);
            Assert.Equal(_mainRebel.Age, result);
        }

        [Fact]
        public async Task MinAsync()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var result = await _rebels.MinAsync(r => r.Age);
            Assert.Equal(_mainRebel.Age, result);
        }

        [Fact]
        public void Sum()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var result = _rebels.Sum(r => r.Age);
            Assert.Equal(_mainRebel.Age, result);
        }

        [Fact]
        public void Average()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var result = _rebels.Average(r => r.Age);
            Assert.Equal(_mainRebel.Age, result);
        }

        [Fact]
        public void Any()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var result = _rebels.Any(r => r.Age == 19);
            Assert.True(result);
        }

        [Fact] public void All()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var result = _rebels.All(r => r.Age == 19);
            Assert.True(result);
        }

        [Fact]
        public void First()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var result = _rebels.First();
            Assert.Equal(_mainRebel.Age, result.Age);
        }

        [Fact]
        public void First_Expr()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var result = _rebels.First(r => r.Age == 19);
            Assert.Equal(_mainRebel.Age, result.Age);
        }

        [Fact]
        public void FirstOrDefault()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
            var result = _rebels.FirstOrDefault();
            Assert.Null(result);
        }

        [Fact]
        public void FirstOrDefault_Expr()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
            var result = _rebels.FirstOrDefault(r => r.Age == 20);
            Assert.Null(result);
        }

        [Fact]
        public void Last()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var result = _rebels.Last();
            Assert.Equal(_mainRebel.Age, result.Age);
        }

        [Fact]
        public void Last_Expr()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var result = _rebels.Last(r => r.Age == 19);
            Assert.Equal(_mainRebel.Age, result.Age);
        }

        [Fact]
        public void LastOrDefault()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
            var result = _rebels.LastOrDefault();
            Assert.Null(result);
        }

        [Fact]
        public void LastOrDefault_Expr()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
            var result = _rebels.LastOrDefault(r => r.Age == 20);
            Assert.Null(result);
        }

        [Fact]
        public void Single()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var result = _rebels.Single();
            Assert.Equal(_mainRebel.Age, result.Age);
        }

        [Fact]
        public void Single_Expr()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(_response);
            var result = _rebels.Single();
            Assert.Equal(_mainRebel.Age, result.Age);
        }

        [Fact]
        public void Single_Exception()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new
            {
                Docs = new List<Rebel>
                    {
                        new Rebel(),
                        new Rebel()
                    }
            });
            var ex = Assert.Throws<InvalidOperationException>(() => _rebels.Single());
            Assert.Equal("Sequence contains more than one element", ex.Message);
        }

        [Fact]
        public void SingleOrDefault()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
            var result = _rebels.SingleOrDefault();
            Assert.Null(result);
        }

        [Fact]
        public void SingleOrDefault_Expr()
        {
            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
            var result = _rebels.SingleOrDefault(r => r.Age == 20);
            Assert.Null(result);
        }
    }
}
