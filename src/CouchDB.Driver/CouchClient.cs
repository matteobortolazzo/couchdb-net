using CouchDB.Driver.Extensions;
using CouchDB.Driver.Types;
using Flurl.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CouchDB.Driver
{
    public class CouchClient : IDisposable
    {
        private DateTime? cookieCreationDate;
        private string cookieToken;
        private readonly CouchSettings settings;
        private readonly FlurlClient flurlClient;
        public string ConnectionString { get; private set; }

        public CouchClient(string connectionString, Action<CouchSettings> configFunc = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
            flurlClient = new FlurlClient(connectionString);
            flurlClient.Configure(s => s.BeforeCall = OnBeforeLogin);
            settings = new CouchSettings();
            configFunc(settings);            
        }
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

            return new CouchDatabase<TSource>(flurlClient, ConnectionString, db);
        }
        public void Dispose()
        {
            flurlClient.Dispose();
        }

        private async Task Login()
        {
            var response = await flurlClient.Request(ConnectionString)
                .AppendPathSegment("_session")
                .PostJsonAsync(new
                {
                    name = settings.Username,
                    password = settings.Password
                });

            cookieCreationDate = DateTime.Now;

            if (response.Headers.TryGetValues("Set-Cookie", out var values))
            {
                var dirtyToken = values.First();
                var regex = new Regex(@"^AuthSession=(.+); Version=1; .*Path=\/; HttpOnly$");
                var match = regex.Match(dirtyToken);
                if (match.Success)
                {
                    cookieToken = match.Groups[1].Value;
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
            switch (settings.AuthenticationType)
            {
                case AuthenticationType.None:
                    break;
                case AuthenticationType.Basic:
                    call.FlurlRequest.WithBasicAuth(settings.Username, settings.Password);
                    break;
                case AuthenticationType.Cookie:
                    var isTokenExpired = 
                        !cookieCreationDate.HasValue || 
                        cookieCreationDate.Value.AddMinutes(settings.CookiesDuration) < DateTime.Now;
                    if (isTokenExpired)
                    {
                        Login().Wait();
                    }
                    call.FlurlRequest.EnableCookies().WithCookie("AuthSession", cookieToken);
                    break;
                default:
                    throw new NotSupportedException($"Authentication of type {settings.AuthenticationType} is not supported.");
            }
        }
    }
}
