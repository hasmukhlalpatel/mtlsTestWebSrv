using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;

namespace mtlsTestWebSrv.Integration.Tests
{
    public class MtlsWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddTransient<FakeCertificateMiddleware>();
            });

            builder.Configure(app =>
            {
                app.UseMiddleware<FakeCertificateMiddleware>();
            });


            builder.ConfigureKestrel(options =>
            {
                options.ConfigureHttpsDefaults(httpsOptions =>
                {
                    httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                    httpsOptions.ClientCertificateValidation = (cert, chain, errors) =>
                    {
                        // Simulate validation logic
                        return cert?.Subject == "CN=TestClient";
                    };
                });
            });
        }
    }

    public class FakeCertificateMiddleware
    {
        private readonly RequestDelegate _next;

        public FakeCertificateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var cert = new X509Certificate2("client.pfx", "password");
            context.Connection.ClientCertificate = cert;
            await _next(context);
        }
    }

}