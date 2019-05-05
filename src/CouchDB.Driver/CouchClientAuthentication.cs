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
        protected virtual void OnBeforeCall(HttpCall httpCall)
        {
            // If session requests no authorization needed
            if (httpCall.Request.RequestUri.ToString().Contains("_session"))
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
                        AsyncContext.Run(() => LoginAsync());
                    }
                    httpCall.FlurlRequest = httpCall.FlurlRequest.EnableCookies().WithCookie("AuthSession", _cookieToken);
                    break;
                default:
                    throw new NotSupportedException($"Authentication of type {_settings.AuthenticationType} is not supported.");
            }
        }

        private async Task LoginAsync()
        {
            HttpResponseMessage response = await _flurlClient.Request(ConnectionString)
                .AppendPathSegment("_session")
                .PostJsonAsync(new
                {
                    name = _settings.Username,
                    password = _settings.Password
                })
                .ConfigureAwait(false);

            _cookieCreationDate = DateTime.Now;

            if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> values))
            {
                var dirtyToken = values.First();
                var regex = new Regex(@"^AuthSession=(.+); Version=1; .*Path=\/; HttpOnly$");
                Match match = regex.Match(dirtyToken);
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
            OperationResult result = await _flurlClient.Request(ConnectionString)
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
