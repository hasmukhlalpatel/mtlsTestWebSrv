using Microsoft.Extensions.Options;

namespace MtlsTestWebSrv;

public static class ClientCertMiddlewareExtensions
{
    public static IApplicationBuilder UseClientCertMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ClientCertificateMiddleware>();
    }
    public static IApplicationBuilder UseClientCertMiddleware(this IApplicationBuilder builder, IOptions<CertificateConfiguration> options)
    {
        return builder.UseMiddleware<ClientCertificateMiddleware>(options);
    }
}