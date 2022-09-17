using System.Security.Cryptography.X509Certificates;
using System.Timers;
using Microsoft.Extensions.Options;

namespace MtlsTestWebSrv
{
    public class ClientCertificateMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CertificateConfiguration _config;
        public ClientCertificateMiddleware(RequestDelegate next, IOptions<CertificateConfiguration> options)
        {
            _next = next;
            _config = options.Value;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                var cert = await context.Connection.GetClientCertificateAsync();
                if (cert == null)
                {
                    await _next.Invoke(context);
                }
                else
                {
                    var publicKey = Convert.ToBase64String(cert.GetPublicKey());
                    var issuer = cert.GetNameInfo(X509NameType.DnsName, true);
                    var subject = cert.GetNameInfo(X509NameType.DnsName, false);
                    if (publicKey == _config.PublicKey)
                    {
                        await _next.Invoke(context);
                    }
                }
            }
            catch
            {
                context.Response.StatusCode = 403;
            }
        }
    }
}
