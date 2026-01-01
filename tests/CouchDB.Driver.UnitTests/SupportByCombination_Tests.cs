using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.UnitTests._Helpers;
using CouchDB.UnitTests.Models;
using Xunit;

namespace CouchDB.Driver.UnitTests;

public class SupportByCombination_Tests : HttpTests
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
        HttpTest.RespondWithJson(_response);
        var result = Rebels.Max(r => r.Age);
        Assert.Equal(_mainRebel.Age, result);
    }

    [Fact]
    public void Min()
    {
        HttpTest.RespondWithJson(_response);
        var result = Rebels.Min(r => r.Age);
        Assert.Equal(_mainRebel.Age, result);
    }

    [Fact]
    public async Task MinAsync()
    {
        HttpTest.RespondWithJson(_response);
        var result = await Rebels.MinAsync(r => r.Age);
        Assert.Equal(_mainRebel.Age, result);
    }

    [Fact]
    public void Sum()
    {
        HttpTest.RespondWithJson(_response);
        var result = Rebels.Sum(r => r.Age);
        Assert.Equal(_mainRebel.Age, result);
    }

    [Fact]
    public void Average()
    {
        HttpTest.RespondWithJson(_response);
        var result = Rebels.Average(r => r.Age);
        Assert.Equal(_mainRebel.Age, result);
    }

    [Fact]
    public void Any()
    {
        HttpTest.RespondWithJson(_response);
        var result = Rebels.Any(r => r.Age == 19);
        Assert.True(result);
    }

    [Fact]
    public void All()
    {
        HttpTest.RespondWithJson(_response);
        var result = Rebels.All(r => r.Age == 19);
        Assert.True(result);
    }

    [Fact]
    public void First()
    {
        HttpTest.RespondWithJson(_response);
        var result = Rebels.First();
        Assert.Equal(_mainRebel.Age, result.Age);
    }

    [Fact]
    public void First_Expr()
    {
        HttpTest.RespondWithJson(_response);
        var result = Rebels.First(r => r.Age == 19);
        Assert.Equal(_mainRebel.Age, result.Age);
    }

    [Fact]
    public void FirstOrDefault()
    {
        HttpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
        var result = Rebels.FirstOrDefault();
        Assert.Null(result);
    }

    [Fact]
    public void FirstOrDefault_Expr()
    {
        HttpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
        var result = Rebels.FirstOrDefault(r => r.Age == 20);
        Assert.Null(result);
    }

    [Fact]
    public void Last()
    {
        HttpTest.RespondWithJson(_response);
        var result = Rebels.Last();
        Assert.Equal(_mainRebel.Age, result.Age);
    }

    [Fact]
    public void Last_Expr()
    {
        HttpTest.RespondWithJson(_response);
        var result = Rebels.Last(r => r.Age == 19);
        Assert.Equal(_mainRebel.Age, result.Age);
    }

    [Fact]
    public void LastOrDefault()
    {
        HttpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
        var result = Rebels.LastOrDefault();
        Assert.Null(result);
    }

    [Fact]
    public void LastOrDefault_Expr()
    {
        HttpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
        var result = Rebels.LastOrDefault(r => r.Age == 20);
        Assert.Null(result);
    }

    [Fact]
    public void Single()
    {
        HttpTest.RespondWithJson(_response);
        var result = Rebels.Single();
        Assert.Equal(_mainRebel.Age, result.Age);
    }

    [Fact]
    public void Single_Expr()
    {
        HttpTest.RespondWithJson(_response);
        var result = Rebels.Single();
        Assert.Equal(_mainRebel.Age, result.Age);
    }

    [Fact]
    public void Single_Exception()
    {
        HttpTest.RespondWithJson(new
        {
            Docs = new List<Rebel>
            {
                new(),
                new()
            }
        });
        var ex = Assert.Throws<InvalidOperationException>(() => Rebels.Single());
        Assert.Equal("Sequence contains more than one element", ex.Message);
    }

    [Fact]
    public void SingleOrDefault()
    {
        HttpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
        var result = Rebels.SingleOrDefault();
        Assert.Null(result);
    }

    [Fact]
    public void SingleOrDefault_Expr()
    {
        HttpTest.RespondWithJson(new { Docs = Array.Empty<Rebel>() });
        var result = Rebels.SingleOrDefault(r => r.Age == 20);
        Assert.Null(result);
    }
}