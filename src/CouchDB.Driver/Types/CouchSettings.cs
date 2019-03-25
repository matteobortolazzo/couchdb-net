using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Driver.Types
{ 
    internal enum AuthenticationType
    {
        None, Basic, Cookie, Proxy
    }

    public class CouchSettings
    {
        internal AuthenticationType AuthenticationType { get; private set; }
        internal string Username { get; private set; }
        internal string Password { get; private set; }
        internal int CookiesDuration { get; private set; }

        internal CouchSettings()
        {
            AuthenticationType = AuthenticationType.None;
        }

        public CouchSettings ConfigureBasicAuthentication(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            AuthenticationType = AuthenticationType.Basic;
            Username = username;
            Password = password;
            return this;
        }
        public CouchSettings ConfigureCookieAuthentication(string username, string password, int cookieDuration = 10)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));
            if (cookieDuration < 1)
                throw new ArgumentException(nameof(cookieDuration), "Cookie duration must be greater than zero.");

            AuthenticationType = AuthenticationType.Cookie;
            Username = username;
            Password = password;
            CookiesDuration = cookieDuration;
            return this;
        }
    }
}
