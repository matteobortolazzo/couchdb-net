using CouchDB.Driver.Options;

namespace CouchDB.Driver.UnitTests._Helpers;

public static class TestCouchClientFactory
{
    public static CouchClient Create(
        HttpTestHelper testHelper,
        CouchCredentials? credentials = null)
    {
        var options = new CouchClientOptions()
        {
            HttpClient = testHelper.HttpClient,
            JsonSerializerOptions = HttpTestHelper.JsonSerializerOptions
        };

        credentials ??= new BasicCredentials("admin", "admin");
        return new CouchClient("http://localhost", credentials, options);
    }
}