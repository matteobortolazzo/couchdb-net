using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Helpers;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;

namespace CouchDB.Driver.Security;

internal class DatabaseSecurity: IDatabaseSecurity
{
    private readonly Func<HttpRequestBuilder> _newRequest;

    internal DatabaseSecurity(Func<HttpRequestBuilder> newRequest)
    {
        _newRequest = newRequest;
    }

    public async Task<DatabaseSecurityInfo> GetInfoAsync()
    {
        return await _newRequest()
            .AppendPathSegment("_security")
            .GetJsonAsync<DatabaseSecurityInfo>()
            .SendRequestAsync()
            .ConfigureAwait(false);
    }

    public async Task SetInfoAsync(DatabaseSecurityInfo info)
    {
        ArgumentNullException.ThrowIfNull(info, nameof(info));

        OperationResult result = await _newRequest()
            .AppendPathSegment("_security")
            .PutJsonAsync(info)
            .ReceiveJson<OperationResult>()
            .SendRequestAsync()
            .ConfigureAwait(false);

        if (!result.Ok)
        {
            throw new CouchDeleteException();
        }
    }
}