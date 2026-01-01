using System;
using System.Collections.Generic;
using System.Linq;
using CouchDB.Driver.UnitTests._Helpers;
using CouchDB.UnitTests.Models;
using Xunit;

namespace CouchDB.Driver.UnitTests.Find;

public class Find_Optimized : HttpTests
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
        HttpTest.RespondWithJson(_response);
        Rebels.FirstOrDefault();
        Assert.Equal(@"{""limit"":1,""selector"":{}}", HttpTest.CallLog[0].RequestBody);
    }

    [Fact]
    public void LastOrDefault()
    {
        HttpTest.RespondWithJson(_response);
        Rebels.LastOrDefault();
        Assert.Equal(@"{""selector"":{}}", HttpTest.CallLog[0].RequestBody);
    }

    [Fact]
    public void FirstOrDefault_Predicate()
    {
        HttpTest.RespondWithJson(_response);
        Rebels.FirstOrDefault(r => r.Age == 19);
        Assert.Equal(@"{""selector"":{""age"":19},""limit"":1}", HttpTest.CallLog[0].RequestBody);
    }

    [Fact]
    public void LastOrDefault_Predicate()
    {
        HttpTest.RespondWithJson(_response);
        Rebels.LastOrDefault(r => r.Age == 19);
        Assert.Equal(@"{""selector"":{""age"":19}}", HttpTest.CallLog[0].RequestBody);
    }

    [Fact]
    public void FirstOrDefault_Predicate_Where()
    {
        HttpTest.RespondWithJson(_response);
        Rebels.Where(c => c.Name == "Luke").FirstOrDefault(r => r.Age == 19);
        Assert.Equal(@"{""selector"":{""$and"":[{""name"":""Luke""},{""age"":19}]},""limit"":1}",
            HttpTest.CallLog[0].RequestBody);
    }

    [Fact]
    public void LastOrDefault_Predicate_Where()
    {
        HttpTest.RespondWithJson(_response);
        Rebels.Where(c => c.Name == "Luke").LastOrDefault(r => r.Age == 19);
        Assert.Equal(@"{""selector"":{""$and"":[{""name"":""Luke""},{""age"":19}]}}", HttpTest.CallLog[0].RequestBody);
    }
}