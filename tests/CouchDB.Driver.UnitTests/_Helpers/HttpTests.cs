using System;
using CouchDB.UnitTests.Models;

namespace CouchDB.Driver.UnitTests._Helpers;

public class HttpTests : IDisposable
{
    protected readonly HttpTestHelper HttpTest;
    protected readonly CouchDatabase<Rebel> Rebels;

    protected HttpTests()
    {
        HttpTest = new HttpTestHelper();
        var client = TestCouchClientFactory.Create(HttpTest);
        Rebels = (CouchDatabase<Rebel>)client.GetDatabase<Rebel>("rebels");
    }

    public void Dispose()
    {
        HttpTest.Dispose();
    }
}