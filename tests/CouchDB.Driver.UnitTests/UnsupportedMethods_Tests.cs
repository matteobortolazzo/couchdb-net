using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouchDB.Driver.UnitTests.Models;
using Flurl.Http.Testing;
using Xunit;

namespace CouchDB.Driver.UnitTests
{
    public class UnsupportedMethods_Tests
    {
        private readonly CouchDatabase<Rebel> _rebels;
        private readonly Rebel _mainRebel;
        private readonly List<Rebel> _rebelsList;
        private object _response;

        public UnsupportedMethods_Tests()
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
        public async Task All()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().All(r => r.Name == _mainRebel.Name);
                Assert.True(result);
            }
        }
        [Fact]
        public async Task Any()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().Any();
                Assert.True(result);
            }
        }
        [Fact]
        public async Task Any_Selector()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().Any(r => r.Name == _mainRebel.Name);
                Assert.True(result);
            }
        }

        [Fact]
        public async Task Avg_Expr()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().Average(r => r.Age);
                Assert.Equal(19, result);
            }
        }

        [Fact]
        public async Task Count()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().Count();
                Assert.Equal(1, result);
            }
        }

        [Fact]
        public async Task CountExpr()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().Count(r => r.Age == 19);
                Assert.Equal(1, result);
            }
        }

        [Fact]
        public async Task DefaultIfEmpty()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.Where(c => c.Name == "Luce").DefaultIfEmpty(_mainRebel);
                Assert.Equal(_mainRebel, result.First());
            }
        }

        [Fact]
        public async Task ElementAt()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().ElementAt(0);
                Assert.Equal(_mainRebel, result);
            }
        }
        [Fact]
        public async Task ElementAtOrDefault()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().ElementAtOrDefault(2);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GroupBy()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().GroupBy(c => c.Id);
                Assert.Equal(1, result.Count());
            }
        }

        [Fact]
        public async Task Last()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().Last();
                Assert.Equal(_mainRebel, result);
            }
        }
        [Fact]
        public async Task Last_Expr()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().Last(c => c.Name == _mainRebel.Name);
                Assert.Equal(_mainRebel, result);
            }
        }

        [Fact]
        public async Task LongCount()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().LongCount(r => r.Age == 19);
                Assert.Equal(1, result);
            }
        }

        [Fact]
        public async Task Reverse()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().Reverse();
                Assert.Equal(_mainRebel, result.First());
            }
        }

        [Fact]
        public async Task SelectMany()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().SelectMany(r => r.Skills).ToList();
                Assert.Single(result);
            }
        }

        [Fact]
        public async Task Sum()
        {
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(_response);
                var result = _rebels.AsQueryable().Sum(r => r.Age);
                Assert.Equal(19, result);
            }
        }
    }
}
