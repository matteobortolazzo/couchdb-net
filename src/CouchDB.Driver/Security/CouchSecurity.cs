using CouchDB.Driver.Helpers;
using Flurl.Http;
using System;
using System.Threading.Tasks;

namespace CouchDB.Driver.Security
{
    public class CouchSecurity
    {
        private Func<IFlurlRequest> _newRequest;

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
                   .SendRequestAsync();
        }

        /// <summary>
        /// Sets security information about the database.
        /// </summary>
        /// <param name="info">The security object to set.</param>
        /// <returns>A task that represents the asynchronous operation. </returns>
        public async Task SetInfoAsync(CouchSecurityInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            await _newRequest()
                   .AppendPathSegment("_security")
                   .PutJsonAsync(info)
                   .SendRequestAsync();
        }
    }
}
