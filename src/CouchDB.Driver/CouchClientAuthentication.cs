using CouchDB.Driver.Types;
using Flurl.Http;
using Nito.AsyncEx;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CouchDB.Driver
{
    public partial class CouchClient
    {
        protected virtual void OnBeforeCall(HttpCall call)
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
                        AsyncContext.Run(() => LoginAsync());
                    }
                    call.FlurlRequest.EnableCookies().WithCookie("AuthSession", _cookieToken);
                    break;
                default:
                    throw new NotSupportedException($"Authentication of type {_settings.AuthenticationType} is not supported.");
            }
        }

        private async Task LoginAsync()
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
        private async Task LogoutAsync()
        {
            await _flurlClient.Request(ConnectionString)
                .AppendPathSegment("_session")
                .DeleteAsync();
        }
    }
}
