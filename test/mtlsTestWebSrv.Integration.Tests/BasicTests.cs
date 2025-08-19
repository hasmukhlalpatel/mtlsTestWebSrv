using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;

namespace mtlsTestWebSrv.Integration.Tests
{
    public class BasicTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public BasicTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            //EndpointRouteBuilderExtensions.IsE2ETestCall = true;
        }

        [Theory]
        [InlineData("/api/events/")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsync(url, JsonContent.Create("{}"));

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
        }


        [Theory]
        [InlineData("/api/events2/")]
        public async Task Get_EndpointsReturnSuccessWithMtlsClientCertificate(string url)
        {
            //var handler = new HttpClientHandler();
            //handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            //handler.ClientCertificates.Add(new X509Certificate2("client.pfx", "password"));
            //var client = new HttpClient(handler);

            var client = _factory.CreateDefaultClient(new ClientCertificateHandler());

            // Act
            var response = await client.PostAsync(url, JsonContent.Create("{}"));

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
        }

        public class ClientCertificateHandler : DelegatingHandler
        {
            public ClientCertificateHandler()
                : base(new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ClientCertificates = { new X509Certificate2("client.pfx", "password") }
                })
            {
            }
        }

    }

    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                ;// Register the fake certificate middleware

            });

            builder.Configure(app =>
            {
                ;// Use the fake certificate middleware to simulate client certificate
            });


            builder.ConfigureKestrel(options =>
            {
                ;
            });
        }
    }


}