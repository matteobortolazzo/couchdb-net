using Flurl.Http.Configuration;
using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace CouchDB.Driver.Helpers
{
    internal class CertClientFactory : DefaultHttpClientFactory
    {
        private readonly Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> _serverCertificateCustomValidationCallback;

        public CertClientFactory(Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> serverCertificateCustomValidationCallback)
        {
            _serverCertificateCustomValidationCallback = serverCertificateCustomValidationCallback;
        }

        public override HttpMessageHandler CreateMessageHandler()
        {
            return new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = _serverCertificateCustomValidationCallback
            };
        }
    }
}
