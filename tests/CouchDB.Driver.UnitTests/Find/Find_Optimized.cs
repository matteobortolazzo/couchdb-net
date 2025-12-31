using System;
using System.Collections.Generic;
using System.Linq;
using CouchDB.Driver.UnitTests._Helpers;
using CouchDB.UnitTests.Models;
using Xunit;

namespace CouchDB.Driver.UnitTests.Find;

public class Find_Optimized : HttpTest
{
    private readonly object _response;

    public Find_Optimized()
    {
        var mainRebel = new Rebel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Luke",
            Age = 19,
            Skills = ["Force"]
        };
        var rebelsList = new List<Rebel>
        {
            mainRebel
        };
        _response = new
        {
            Docs = rebelsList
        };
    }

    [Fact]
    public void FirstOrDefault()
    {
        httpTest.RespondWithJson(_response);
        _rebels.FirstOrDefault();
        Assert.Equal(@"{""limit"":1,""selector"":{}}", httpTest.CallLog[0].RequestBody);
    }

    [Fact]
    public void LastOrDefault()
    {
        httpTest.RespondWithJson(_response);
        _rebels.LastOrDefault();
        Assert.Equal(@"{""selector"":{}}", httpTest.CallLog[0].RequestBody);
    }

    [Fact]
    public void FirstOrDefault_Predicate()
    {
        httpTest.RespondWithJson(_response);
        _rebels.FirstOrDefault(r => r.Age == 19);
        Assert.Equal(@"{""selector"":{""age"":19},""limit"":1}", httpTest.CallLog[0].RequestBody);
    }

    [Fact]
    public void LastOrDefault_Predicate()
    {
        httpTest.RespondWithJson(_response);
        _rebels.LastOrDefault(r => r.Age == 19);
        Assert.Equal(@"{""selector"":{""age"":19}}", httpTest.CallLog[0].RequestBody);
    }

    [Fact]
    public void FirstOrDefault_Predicate_Where()
    {
        httpTest.RespondWithJson(_response);
        _rebels.Where(c => c.Name == "Luke").FirstOrDefault(r => r.Age == 19);
        Assert.Equal(@"{""selector"":{""$and"":[{""name"":""Luke""},{""age"":19}]},""limit"":1}",
            httpTest.CallLog[0].RequestBody);
    }

    [Fact]
    public void LastOrDefault_Predicate_Where()
    {
        httpTest.RespondWithJson(_response);
        _rebels.Where(c => c.Name == "Luke").LastOrDefault(r => r.Age == 19);
        Assert.Equal(@"{""selector"":{""$and"":[{""name"":""Luke""},{""age"":19}]}}", httpTest.CallLog[0].RequestBody);
    }
}