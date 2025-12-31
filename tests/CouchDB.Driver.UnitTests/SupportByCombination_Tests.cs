using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.UnitTests._Helpers;
using CouchDB.UnitTests.Models;
using Xunit;

namespace CouchDB.Driver.UnitTests;

public class SupportByCombination_Tests : HttpTest
{
    private readonly Rebel _mainRebel;
    private readonly object _response;

    public SupportByCombination_Tests()
    {
        _mainRebel = new Rebel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Luke",
            Age = 19,
            Skills = ["Force"]
        };
        List<Rebel> rebelsList = [_mainRebel];
        _response = new
        {
            Docs = rebelsList
        };
    }

    [Fact]
    public void Max()
    {
        httpTest.RespondWithJson(_response);
        var result = _rebels.Max(r => r.Age);
        Assert.Equal(_mainRebel.Age, result);
    }

    [Fact]
    public void Min()
    {
        httpTest.RespondWithJson(_response);
        var result = _rebels.Min(r => r.Age);
        Assert.Equal(_mainRebel.Age, result);
    }

    [Fact]
    public async Task MinAsync()
    {
        httpTest.RespondWithJson(_response);
        var result = await _rebels.MinAsync(r => r.Age);
        Assert.Equal(_mainRebel.Age, result);
    }

    [Fact]
    public void Sum()
    {
        httpTest.RespondWithJson(_response);
        var result = _rebels.Sum(r => r.Age);
        Assert.Equal(_mainRebel.Age, result);
    }

    [Fact]
    public void Average()
    {
        httpTest.RespondWithJson(_response);
        var result = _rebels.Average(r => r.Age);
        Assert.Equal(_mainRebel.Age, result);
    }

    [Fact]
    public void Any()
    {
        httpTest.RespondWithJson(_response);
        var result = _rebels.Any(r => r.Age == 19);
        Assert.True(result);
    }

    [Fact]
    public void All()
    {
        httpTest.RespondWithJson(_response);
        var result = _rebels.All(r => r.Age == 19);
        Assert.True(result);
    }

    [Fact]
    public void First()
    {
        httpTest.RespondWithJson(_response);
        var result = _rebels.First();
        Assert.Equal(_mainRebel.Age, result.Age);
    }

    [Fact]
    public void First_Expr()
    {
        httpTest.RespondWithJson(_response);
        var result = _rebels.First(r => r.Age == 19);
        Assert.Equal(_mainRebel.Age, result.Age);
    }

    [Fact]
    public void FirstOrDefault()
    {
        httpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
        var result = _rebels.FirstOrDefault();
        Assert.Null(result);
    }

    [Fact]
    public void FirstOrDefault_Expr()
    {
        httpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
        var result = _rebels.FirstOrDefault(r => r.Age == 20);
        Assert.Null(result);
    }

    [Fact]
    public void Last()
    {
        httpTest.RespondWithJson(_response);
        var result = _rebels.Last();
        Assert.Equal(_mainRebel.Age, result.Age);
    }

    [Fact]
    public void Last_Expr()
    {
        httpTest.RespondWithJson(_response);
        var result = _rebels.Last(r => r.Age == 19);
        Assert.Equal(_mainRebel.Age, result.Age);
    }

    [Fact]
    public void LastOrDefault()
    {
        httpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
        var result = _rebels.LastOrDefault();
        Assert.Null(result);
    }

    [Fact]
    public void LastOrDefault_Expr()
    {
        httpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
        var result = _rebels.LastOrDefault(r => r.Age == 20);
        Assert.Null(result);
    }

    [Fact]
    public void Single()
    {
        httpTest.RespondWithJson(_response);
        var result = _rebels.Single();
        Assert.Equal(_mainRebel.Age, result.Age);
    }

    [Fact]
    public void Single_Expr()
    {
        httpTest.RespondWithJson(_response);
        var result = _rebels.Single();
        Assert.Equal(_mainRebel.Age, result.Age);
    }

    [Fact]
    public void Single_Exception()
    {
        httpTest.RespondWithJson(new
        {
            Docs = new List<Rebel>
            {
                new(),
                new()
            }
        });
        var ex = Assert.Throws<InvalidOperationException>(() => _rebels.Single());
        Assert.Equal("Sequence contains more than one element", ex.Message);
    }

    [Fact]
    public void SingleOrDefault()
    {
        httpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
        var result = _rebels.SingleOrDefault();
        Assert.Null(result);
    }

    [Fact]
    public void SingleOrDefault_Expr()
    {
        httpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
        var result = _rebels.SingleOrDefault(r => r.Age == 20);
        Assert.Null(result);
    }
}