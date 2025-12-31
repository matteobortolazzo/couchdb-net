using System;
using System.Linq;
using CouchDB.Driver.UnitTests._Helpers;
using Xunit;

namespace CouchDB.Driver.UnitTests.Find;

public class Find_Unsupported : HttpTest
{
    [Fact]
    public void ToList_WhereCount_Exception()
    {
        void CountQuery() => _rebels
            .Where(u => u.Battles.Count > 0)
            .ToString();

        Assert.Throws<NotSupportedException>(CountQuery);
    }
}