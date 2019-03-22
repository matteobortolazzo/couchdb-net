using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CouchDB.Client.Helpers;
using CouchDB.Client.Responses;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;

namespace CouchDB.Client
{
    public class CouchClient
    {
        private class AuthorizationData
        {
            public bool UseCookieAuthentication { get; set; }
            public bool UseBasicAuthentication { get; set; }
            public string AuthName { get; set; }
            public string AuthPassword { get; set; }
            public string AuthToken { get; set; }
            public DateTime AuthTokenDate { get; set; }
            public int AuthTokenDuration { get; set; }
        }

        private readonly AuthorizationData _authData;
        private readonly string _serverUrl;
        private IFlurlClient _flurlClient;

        public string ServerUrl => _serverUrl;

        internal IFlurlRequest NewRequest()
        {
            if (_authData.UseCookieAuthentication && (_authData.AuthToken == null ||
                                                 _authData.AuthTokenDate.AddMinutes(_authData.AuthTokenDuration) <
                                                 DateTime.Now))
            {
                Login().Wait();
            }

            var request = _flurlClient.Request(_serverUrl);

            if (_authData.UseBasicAuthentication)
            {
                request = request.WithBasicAuth(_authData.AuthName, _authData.AuthPassword);
            }
            else if (_authData.UseCookieAuthentication)
            {
                request = request.EnableCookies().WithCookie("AuthSession", _authData.AuthToken);
            }

            return request;
        }

        public static string ToBase64Encoded(string value)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value));
        }

        public CouchClient(string serverUrl)
        {
            _serverUrl = serverUrl;
            _authData = new AuthorizationData();

            _flurlClient = FlurlHttp.GlobalSettings.FlurlClientFactory.Get(_serverUrl);
        }

        #region Authentication

        public void ConfigureAuthentication(string name, string password, int tokenDurationMinutes = 10)
        {
            _authData.UseCookieAuthentication = true;
            _authData.UseBasicAuthentication = false;
            _authData.AuthName = name;
            _authData.AuthPassword = password;
            _authData.AuthTokenDuration = tokenDurationMinutes;
        }

        public void ConfigureBasicAuthentication(string name, string password)
        {
            _authData.UseCookieAuthentication = false;
            _authData.UseBasicAuthentication = true;
            _authData.AuthName = name;
            _authData.AuthPassword = password;
        }

        private async Task Login()
        {
            var response = await _flurlClient.Request(_serverUrl)
                .AppendPathSegment("_session")
                .PostJsonAsync(new
                {
                    name = _authData.AuthName,
                    password = _authData.AuthPassword
                });

            _authData.AuthTokenDate = DateTime.Now;

            if (response.Headers.TryGetValues("Set-Cookie", out var values))
            {
                var dirtyToken = values.First();
                var regex = new Regex(@"^AuthSession=(.+); Version=1; (.+)Path=/; HttpOnly$");
                var match = regex.Match(dirtyToken);
                if (match.Success)
                {
                    _authData.AuthToken = match.Groups[1].Value;
                    return;
                }
            }

            throw new InvalidOperationException("Error while trying to log-in.");
        }

        #endregion

        #region Databases

        public async Task<IEnumerable<string>> GetDatabasesNamesAsync()
        {
            return await NewRequest()
                .AppendPathSegment("_all_dbs")
                .GetJsonAsync<IEnumerable<string>>()
                .SendAsync();
        }

        public CouchDatabase<T> GetDatabase<T>(string dbName) where T : CouchEntity
        {
            return new CouchDatabase<T>(this, dbName);
        }

        public async Task AddDatabaseAsync(string dbName)
        {
            await NewRequest()
                .AppendPathSegment(dbName)
                .PutAsync(null)
                .SendAsync();
        }

        public async Task RemoveDatabaseAsync(string dbName)
        {
            await NewRequest()
                .AppendPathSegment(dbName)
                .DeleteAsync()
                .SendAsync();
        }

        #endregion

        public async Task<IEnumerable<CouchActiveTask>> GetActiveTasksAsync()
        {
            return await NewRequest()
                .AppendPathSegment("_active_tasks")
                .GetJsonAsync<IEnumerable<CouchActiveTask>>()
                .SendAsync();
        }
    }
}
