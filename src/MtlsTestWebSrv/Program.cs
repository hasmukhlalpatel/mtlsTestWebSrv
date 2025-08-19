using System.Net.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using MtlsTestWebSrv;

var builder = WebApplication.CreateBuilder(args);

var certConfig = builder.Configuration.GetSection("CertificateConfiguration");
builder.Services.Configure<CertificateConfiguration>(certConfig);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate(options =>
    {
        options.AllowedCertificateTypes = CertificateTypes.All;
        options.Events = new CertificateAuthenticationEvents
        {
            OnCertificateValidated = context =>
            {
                var claims = new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        context.ClientCertificate.Subject,
                        ClaimValueTypes.String, context.Options.ClaimsIssuer),
                    new Claim(
                        ClaimTypes.Name,
                        context.ClientCertificate.Subject,
                        ClaimValueTypes.String, context.Options.ClaimsIssuer)
                };

                context.Principal = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, context.Scheme.Name));
                context.Success();

                return Task.CompletedTask;
            }
        };
    }).AddCertificateCache(opt =>
    {
        opt.CacheSize = 1024;
        opt.CacheEntryExpiration= TimeSpan.FromMinutes(2);
    });

//builder.Services.AddAuthorization();

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.ConfigureHttpsDefaults(options =>
    {
        options.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
        options.ClientCertificateValidation += (X509Certificate2, X509Chain, SslPolicyErrors) =>
        {
            return true;
        };
    });
});

var app = builder.Build();

app.UseClientCertMiddleware();

app.UseCertificateForwarding();

app.UseAuthentication();

//app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/", () => "Hello World!");
//app.MapPost("/ProvWeb/ezapi/CollectionsAPI/request", () => "Hello World!");
app.Run();


public partial class Program { } // For testing purposes