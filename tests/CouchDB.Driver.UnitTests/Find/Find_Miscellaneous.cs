using CouchDB.UnitTests.Models;
using System;
using System.Linq;
using Xunit;
using CouchDB.Driver.Query.Extensions;
using CouchDB.Driver.UnitTests._Helpers;

namespace CouchDB.Driver.UnitTests.Find;

public class Find_Miscellaneous : HttpTests
{
    [Fact]
    public void Limit()
    {
        var json = Rebels.Take(20).ToString();
        Assert.Equal(@"{""limit"":20,""selector"":{}}", json);
    }

    [Fact]
    public void Skip()
    {
        var json = Rebels.Skip(20).ToString();
        Assert.Equal(@"{""skip"":20,""selector"":{}}", json);
    }

    [Fact]
    public void Field()
    {
        var json = Rebels.Select(r => r.Age).ToString();
        Assert.Equal(@"{""fields"":[""age""],""selector"":{}}", json);
    }

    [Fact]
    public void Fields()
    {
        var json = Rebels.Select(
                r => r.Name,
                r => r.Age,
                r => r.Vehicle.CanFly)
            .ToString();
        Assert.Equal(@"{""fields"":[""name"",""age"",""vehicle.canFly""],""selector"":{}}", json);
    }

    [Fact]
    public void Fields_NewObject()
    {
        var json = Rebels.Select(r => new
            {
                r.Name,
                r.Age,
                r.Vehicle.CanFly
            })
            .ToString();
        Assert.Equal(@"{""fields"":[""name"",""age"",""vehicle.canFly""],""selector"":{}}", json);
    }

    [Fact]
    public void Convert()
    {
        var json = Rebels.Convert<Rebel, SimpleRebel>()
            .ToString();
        Assert.Equal(@"{""fields"":[""name"",""age""],""selector"":{}}", json);
    }

    [Fact]
    public void Index_Partial()
    {
        var json = Rebels.UseIndex("design_document").ToString();
        Assert.Equal(@"{""use_index"":""design_document"",""selector"":{}}", json);
    }

    [Fact]
    public void Index_Complete()
    {
        var json = Rebels.UseIndex("design_document", "index_name").ToString();
        Assert.Equal(@"{""use_index"":[""design_document"",""index_name""],""selector"":{}}", json);
    }

    [Fact]
    public void Index_TooMuchArguments()
    {
        var exception = Record.Exception(() =>
        {
            var json = Rebels.UseIndex("arg1", "arg2", "arg3").ToString();
        });
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Quorum()
    {
        var json = Rebels.WithReadQuorum(20).ToString();
        Assert.Equal(@"{""r"":20,""selector"":{}}", json);
    }

    [Fact]
    public void Bookmark()
    {
        var json = Rebels.UseBookmark("g1AAAABweJzLY...").ToString();
        Assert.Equal(@"{""bookmark"":""g1AAAABweJzLY..."",""selector"":{}}", json);
    }

    [Fact]
    public void Update()
    {
        var json = Rebels.WithoutIndexUpdate().ToString();
        Assert.Equal(@"{""update"":false,""selector"":{}}", json);
    }

    [Fact]
    public void Stable()
    {
        var json = Rebels.FromStable().ToString();
        Assert.Equal(@"{""stable"":true,""selector"":{}}", json);
    }

    [Fact]
    public void ExecutionStats()
    {
        var json = Rebels.IncludeExecutionStats().ToString();
        Assert.Equal(@"{""execution_stats"":true,""selector"":{}}", json);
    }

    [Fact]
    public void Conflicts()
    {
        var json = Rebels.IncludeConflicts().ToString();
        Assert.Equal(@"{""conflicts"":true,""selector"":{}}", json);
    }

    [Fact]
    public void Boolean_ShortCircuit_Simple()
    {
        bool useFilter = true;
        var json = Rebels.Where(_ => useFilter).ToString();
        Assert.Equal(@"{""selector"":{}}", json);
    }

    [Fact]
    public void Boolean_ShortCircuit_AndTrue()
    {
        bool useFilter = true;
        var json = Rebels.Where(r => useFilter && r.Age == 19).Take(1).ToString();
        Assert.Equal(@"{""selector"":{""age"":19},""limit"":1}", json);
    }

    [Fact]
    public void Boolean_ShortCircuit_AndFalse()
    {
        bool useFilter = false;
        var json = Rebels.Where(r => useFilter && r.Age == 19).Take(1).ToString();
        Assert.Equal(@"{""limit"":1,""selector"":{}}", json);
    }

    [Fact]
    public void Boolean_ShortCircuit_OrTrue()
    {
        bool useFilter = true;
        var json = Rebels.Where(r => useFilter || r.Age == 19).ToString();
        Assert.Equal(@"{""selector"":{}}", json);
    }

    [Fact]
    public void Boolean_ShortCircuit_Complex()
    {
        bool useFilter1 = true;
        bool useFilter2 = false;
        var json = Rebels.Where(r => (useFilter1 && r.Age == 19) || useFilter2).ToString();
        Assert.Equal(@"{""selector"":{""age"":19}}", json);
    }

    [Fact]
    public void Boolean_ShortCircuit_MultiWhere1()
    {
        bool useFilter = true;
        var json = Rebels.Where(r => r.Age == 19).Where(_ => useFilter).ToString();
        Assert.Equal(@"{""selector"":{""age"":19}}", json);
    }

    [Fact]
    public void Boolean_ShortCircuit_MultiWhere2()
    {
        bool useFilter = true;
        var json = Rebels.Where(_ => useFilter).Where(r => r.Age == 19).ToString();
        Assert.Equal(@"{""selector"":{""age"":19}}", json);
    }

    [Fact]
    public void Combinations()
    {
        var json = Rebels
            .Where(r =>
                r.Surname == "Skywalker" &&
                (
                    r.Battles.All(b => b.Planet == "Naboo") ||
                    r.Battles.Any(b => b.Planet == "Death Star")
                )
            )
            .OrderByDescending(r => r.Name)
            .ThenByDescending(r => r.Age)
            .Skip(1)
            .Take(2)
            .WithReadQuorum(2)
            .UseBookmark("g1AAAABweJzLY...")
            .WithReadQuorum(150)
            .WithoutIndexUpdate()
            .FromStable()
            .IncludeExecutionStats()
            .Select(r => new
            {
                r.Name,
                r.Age,
                r.Species
            }).ToString();
        Assert.Equal(
            @"{""selector"":{""$and"":[{""surname"":""Skywalker""},{""$or"":[{""battles"":{""$allMatch"":{""planet"":""Naboo""}}},{""battles"":{""$elemMatch"":{""planet"":""Death Star""}}}]}]},""sort"":[{""name"":""desc""},{""age"":""desc""}],""skip"":1,""limit"":2,""r"":2,""bookmark"":""g1AAAABweJzLY..."",""r"":150,""update"":false,""stable"":true,""execution_stats"":true,""fields"":[""name"",""age"",""species""]}",
            json);
    }
}