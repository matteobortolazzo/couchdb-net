using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.UnitTests._Helpers;
using Xunit;

namespace CouchDB.Driver.UnitTests.Find;

public class Find_Projection_Tests : HttpTests
{
    #region Anonymous projections (regression - already fixed by FindResult converter)

    [Fact]
    public async Task Select_AnonymousId_MapsId()
    {
        HttpTest.RespondWith("""{"docs":[{"_id":"abc"}],"bookmark":"bm"}""");

        var result = await Rebels.Select(x => new { x.Id }).ToListAsync();

        Assert.Single(result);
        Assert.Equal("abc", result[0].Id);
    }

    [Fact]
    public async Task Select_AnonymousIdRev_MapsIdAndRev()
    {
        HttpTest.RespondWith("""{"docs":[{"_id":"abc","_rev":"1-xyz"}],"bookmark":"bm"}""");

        var result = await Rebels.Select(x => new { x.Id, x.Rev }).ToListAsync();

        Assert.Single(result);
        Assert.Equal("abc", result[0].Id);
        Assert.Equal("1-xyz", result[0].Rev);
    }

    [Fact]
    public async Task Select_AnonymousIdAndField_MapsAll()
    {
        HttpTest.RespondWith("""{"docs":[{"_id":"abc","name":"Luke"}],"bookmark":"bm"}""");

        var result = await Rebels.Select(x => new { x.Id, x.Name }).ToListAsync();

        Assert.Single(result);
        Assert.Equal("abc", result[0].Id);
        Assert.Equal("Luke", result[0].Name);
    }

    #endregion

    #region Scalar projections (the fix)

    [Fact]
    public async Task Select_ScalarId_ReturnsIds()
    {
        HttpTest.RespondWith("""{"docs":[{"_id":"abc"},{"_id":"def"}],"bookmark":"bm"}""");

        var result = await Rebels.Select(x => x.Id).ToListAsync();

        Assert.Equal(["abc", "def"], result);
    }

    [Fact]
    public async Task Select_ScalarName_ReturnsNames()
    {
        HttpTest.RespondWith("""{"docs":[{"name":"Luke"},{"name":"Leia"}],"bookmark":"bm"}""");

        var result = await Rebels.Select(x => x.Name).ToListAsync();

        Assert.Equal(["Luke", "Leia"], result);
    }

    [Fact]
    public void Select_ScalarValueType_ReturnsValues()
    {
        HttpTest.RespondWith("""{"docs":[{"age":19},{"age":21}],"bookmark":"bm"}""");

        // Value-type projections cannot use ToListAsync (constrained to reference types),
        // so they materialize through synchronous enumeration.
        List<int> result = Rebels.Select(x => x.Age).ToList();

        Assert.Equal([19, 21], result);
    }

    #endregion
}
