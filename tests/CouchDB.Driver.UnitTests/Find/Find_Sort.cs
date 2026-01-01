using System;
using Xunit;
using System.Linq;
using CouchDB.Driver.UnitTests._Helpers;

namespace CouchDB.Driver.UnitTests.Find;

public class Find_Sort : HttpTests
{
    [Fact]
    public void SortAsc()
    {
        var json = Rebels.OrderBy(c => c.Name).ToString();
        Assert.Equal(@"{""sort"":[""name""],""selector"":{}}", json);
    }

    [Fact]
    public void SortDesc()
    {
        var json = Rebels.OrderByDescending(c => c.Name).ToString();
        Assert.Equal(@"{""sort"":[{""name"":""desc""}],""selector"":{}}", json);
    }

    [Fact]
    public void SortAscMultiple()
    {
        var json = Rebels.OrderBy(c => c.Name).ThenBy(c => c.Age).ToString();
        Assert.Equal(@"{""sort"":[""name"",""age""],""selector"":{}}", json);
    }

    [Fact]
    public void SortDescMultiple()
    {
        var json = Rebels.OrderByDescending(c => c.Name).ThenByDescending(c => c.Age).ToString();
        Assert.Equal(@"{""sort"":[{""name"":""desc""},{""age"":""desc""}],""selector"":{}}", json);
    }

    [Fact]
    public void SortAscMultiple_DifferentDirections()
    {
        var exception =
            Record.Exception(() => { Rebels.OrderBy(c => c.Name).ThenByDescending(c => c.Age).ToString(); });
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void SortDescMultiple_DifferentDirections()
    {
        var exception =
            Record.Exception(() => { Rebels.OrderByDescending(c => c.Name).ThenBy(c => c.Age).ToString(); });
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }
}