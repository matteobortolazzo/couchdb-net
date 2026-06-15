using System.Linq;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.UnitTests._Helpers;
using CouchDB.UnitTests.Models;
using Xunit;

namespace CouchDB.Driver.UnitTests.Find;

public class Find_IdRevMapping_Tests : HttpTests
{
    private const string FindResponseJson =
        """{"docs":[{"_id":"abc123","_rev":"1-xyz","name":"Luke","age":19}],"bookmark":"bookmark1"}""";

    private const string FindResponseMultipleDocsJson =
        """{"docs":[{"_id":"id1","_rev":"1-aaa","name":"Luke","age":19},{"_id":"id2","_rev":"2-bbb","name":"Leia","age":21}],"bookmark":"bookmark2"}""";

    [Fact]
    public async Task ToListAsync_MapsIdAndRevFromCouchDbFields()
    {
        HttpTest.RespondWith(FindResponseJson);

        var result = await Rebels.Where(r => r.Age == 19).ToListAsync();

        Assert.Single(result);
        Assert.Equal("abc123", result[0].Id);
        Assert.Equal("1-xyz", result[0].Rev);
    }

    [Fact]
    public async Task ToListAsync_MapsMultipleDocuments_IdAndRevCorrectly()
    {
        HttpTest.RespondWith(FindResponseMultipleDocsJson);

        var result = await Rebels.ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("id1", result[0].Id);
        Assert.Equal("1-aaa", result[0].Rev);
        Assert.Equal("id2", result[1].Id);
        Assert.Equal("2-bbb", result[1].Rev);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_MapsIdAndRevFromCouchDbFields()
    {
        HttpTest.RespondWith(FindResponseJson);

        var result = await Rebels.Where(r => r.Age == 19).FirstOrDefaultAsync();

        Assert.NotNull(result);
        Assert.Equal("abc123", result.Id);
        Assert.Equal("1-xyz", result.Rev);
    }

    [Fact]
    public async Task ToListAsync_WithoutIdRev_DoesNotBreakDeserialization()
    {
        HttpTest.RespondWith("""{"docs":[{"name":"Luke","age":19}],"bookmark":"bm"}""");

        var result = await Rebels.ToListAsync();

        Assert.Single(result);
        Assert.Equal("Luke", result[0].Name);
        Assert.Equal(19, result[0].Age);
    }
}
