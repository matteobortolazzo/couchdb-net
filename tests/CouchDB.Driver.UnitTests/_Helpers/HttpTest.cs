using System;
using CouchDB.UnitTests.Models;

namespace CouchDB.Driver.UnitTests._Helpers;

public class HttpTest : IDisposable
{
    protected readonly HttpTestHelper httpTest;
    protected readonly ICouchDatabase<Rebel> _rebels;

    protected HttpTest()
    {
        httpTest = new HttpTestHelper();
        var client = TestCouchClientFactory.Create(httpTest);
        _rebels = client.GetDatabase<Rebel>();
    }

    public void Dispose()
    {
        httpTest.Dispose();
    }
}