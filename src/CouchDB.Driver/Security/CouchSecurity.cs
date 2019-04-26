using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Helpers;
using Flurl.Http;
using System;
using System.Threading.Tasks;

namespace CouchDB.Driver.Security
{
    public class CouchSecurity
    {
        private readonly Func<IFlurlRequest> _newRequest;

        internal CouchSecurity(Func<IFlurlRequest> newRequest)
        {
            _newRequest = newRequest;
        }

        /// <summary>
        /// Gets security information about the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the database security information.</returns>
        public async Task<CouchSecurityInfo> GetInfoAsync()
        {
            return await _newRequest()
                   .AppendPathSegment("_security")
                   .GetJsonAsync<CouchSecurityInfo>()
                   .SendRequestAsync()
                   .ConfigureAwait(false);
        }

        /// <summary>
        /// Sets security information about the database.
        /// </summary>
        /// <param name="info">The security object to set.</param>
        /// <returns>A task that represents the asynchronous operation. </returns>
        public async Task SetInfoAsync(CouchSecurityInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

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
}
