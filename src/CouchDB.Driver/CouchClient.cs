using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Types;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CouchDB.Driver
{
    public class CouchClient : IDisposable
    {
        private DateTime? _cookieCreationDate;
        private string _cookieToken;
        private readonly CouchSettings _settings;
        private readonly FlurlClient _flurlClient;
        public string ConnectionString { get; private set; }

        public CouchClient(string connectionString, Action<CouchSettings> configFunc = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
            _flurlClient = new FlurlClient(connectionString);
            _flurlClient.Configure(s => {
                s.BeforeCall = OnBeforeLogin;
                if (_settings.ServerCertificateCustomValidationCallback != null)
                {
                    s.HttpClientFactory = new CertClientFactory(_settings.ServerCertificateCustomValidationCallback);
                }
            });
            _settings = new CouchSettings();
            configFunc?.Invoke(_settings);
        }

        #region Operations

        public CouchDatabase<TSource> GetDatabase<TSource>() where TSource : CouchEntity
        {
            var type = typeof(TSource);
            var db = type.GetName();
            return GetDatabase<TSource>(db);
        }
        public CouchDatabase<TSource> GetDatabase<TSource>(string db) where TSource : CouchEntity
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            return new CouchDatabase<TSource>(_flurlClient, ConnectionString, db);
        }
        public async Task<IEnumerable<string>> GetDatabasesNamesAsync()
        {
            return await NewRequest()
                .AppendPathSegment("_all_dbs")
                .GetJsonAsync<IEnumerable<string>>()
                .SendRequestAsync();
        }        
        public async Task AddDatabaseAsync(string db)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            await NewRequest()
                .AppendPathSegment(db)
                .PutAsync(null)
                .SendRequestAsync();
        }
        public async Task RemoveDatabaseAsync(string db)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            await NewRequest()
                .AppendPathSegment(db)
                .DeleteAsync()
                .SendRequestAsync();
        }

        #endregion

        #region Helpers

        private async Task Login()
        {
            var response = await _flurlClient.Request(ConnectionString)
                .AppendPathSegment("_session")
                .PostJsonAsync(new
                {
                    name = _settings.Username,
                    password = _settings.Password
                });

            _cookieCreationDate = DateTime.Now;

            if (response.Headers.TryGetValues("Set-Cookie", out var values))
            {
                var dirtyToken = values.First();
                var regex = new Regex(@"^AuthSession=(.+); Version=1; .*Path=\/; HttpOnly$");
                var match = regex.Match(dirtyToken);
                if (match.Success)
                {
                    _cookieToken = match.Groups[1].Value;
                    return;
                }
            }

            throw new InvalidOperationException("Error while trying to log-in.");
        }
        private void OnBeforeLogin(HttpCall call)
        {
            // If cookie request
            if (call.Request.RequestUri.ToString().Contains("_session") && call.Request.Method == HttpMethod.Post)
            {
                return;
            }
            switch (_settings.AuthenticationType)
            {
                case AuthenticationType.None:
                    break;
                case AuthenticationType.Basic:
                    call.FlurlRequest.WithBasicAuth(_settings.Username, _settings.Password);
                    break;
                case AuthenticationType.Cookie:
                    var isTokenExpired = 
                        !_cookieCreationDate.HasValue || 
                        _cookieCreationDate.Value.AddMinutes(_settings.CookiesDuration) < DateTime.Now;
                    if (isTokenExpired)
                    {
                        Login().Wait();
                    }
                    call.FlurlRequest.EnableCookies().WithCookie("AuthSession", _cookieToken);
                    break;
                default:
                    throw new NotSupportedException($"Authentication of type {_settings.AuthenticationType} is not supported.");
            }
        }
        private IFlurlRequest NewRequest()
        {
            return _flurlClient.Request(ConnectionString);
        }

        #endregion

        #region Implementations

        public void Dispose()
        {
            _flurlClient.Dispose();
        }

        #endregion
    }
}
