using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using CouchDB.Driver.Settings;
using Flurl.Http;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CouchDB.Driver
{
    public partial class CouchClient
    {
        protected virtual async Task OnBeforeCallAsync(HttpCall httpCall)
        {
            if (httpCall == null)
            {
                throw new ArgumentNullException(nameof(httpCall));
            }

            // If session requests no authorization needed
            if (httpCall.Request?.RequestUri?.ToString()?.Contains("_session", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                return;
            }
            switch (_settings.AuthenticationType)
            {
                case AuthenticationType.None:
                    break;
                case AuthenticationType.Basic:
                    httpCall.FlurlRequest = httpCall.FlurlRequest.WithBasicAuth(_settings.Username, _settings.Password);
                    break;
                case AuthenticationType.Cookie:
                    var isTokenExpired =
                        !_cookieCreationDate.HasValue ||
                        _cookieCreationDate.Value.AddMinutes(_settings.CookiesDuration) < DateTime.Now;
                    if (isTokenExpired)
                    {
                        await LoginAsync().ConfigureAwait(false);
                    }
                    httpCall.FlurlRequest = httpCall.FlurlRequest.EnableCookies().WithCookie("AuthSession", _cookieToken);
                    break;
                case AuthenticationType.Proxy:
                    httpCall.FlurlRequest = httpCall.FlurlRequest.WithHeader("X-Auth-CouchDB-UserName", _settings.Username)
                        .WithHeader("X-Auth-CouchDB-Roles", string.Join(",", _settings.Roles));
                    if (_settings.Password != null)
                    {
                        httpCall.FlurlRequest = httpCall.FlurlRequest.WithHeader("X-Auth-CouchDB-Token", _settings.Password);
                    }
                    break;
                default:
                    throw new NotSupportedException($"Authentication of type {_settings.AuthenticationType} is not supported.");
            }
        }

        private async Task LoginAsync()
        {
            HttpResponseMessage response = await _flurlClient.Request(DatabaseUri)
                .AppendPathSegment("_session")
                .PostJsonAsync(new
                {
                    name = _settings.Username,
                    password = _settings.Password
                })
                .ConfigureAwait(false);

            _cookieCreationDate = DateTime.Now;

            if (!response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> values))
            {
                throw new InvalidOperationException("Error while trying to log-in.");
            }

            var dirtyToken = values.First();
            var regex = new Regex(@"^AuthSession=(.+); Version=1; .*Path=\/; HttpOnly$");
            Match match = regex.Match(dirtyToken);
            if (!match.Success)
            {
                throw new InvalidOperationException("Error while trying to log-in.");
            }

            _cookieToken = match.Groups[1].Value;
        }

        private async Task LogoutAsync()
        {
            OperationResult result = await _flurlClient.Request(DatabaseUri)
                .AppendPathSegment("_session")
                .DeleteAsync()
                .ReceiveJson<OperationResult>()
                .ConfigureAwait(false);

            if (!result.Ok)
            {
                throw new CouchDeleteException();
            }
        }
    }
}
