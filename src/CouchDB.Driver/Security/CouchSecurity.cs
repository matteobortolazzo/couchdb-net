using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Helpers;
using System.Threading.Tasks;
using CouchDB.Driver.Extensions;

namespace CouchDB.Driver.Security;

internal class CouchSecurity: ICouchSecurity
{
    private readonly Func<HttpRequestBuilder> _newRequest;

    internal CouchSecurity(Func<HttpRequestBuilder> newRequest)
    {
        _newRequest = newRequest;
    }

    public async Task<CouchSecurityInfo> GetInfoAsync()
    {
        return await _newRequest()
            .AppendPathSegment("_security")
            .GetJsonAsync<CouchSecurityInfo>()
            .SendRequestAsync()
            .ConfigureAwait(false);
    }

    public async Task SetInfoAsync(CouchSecurityInfo info)
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