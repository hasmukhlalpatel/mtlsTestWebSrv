# Sample mtls Test WebSrv
## 🛠️ Step-by-Step: Setting Up mTLS
## 1. Generate Certificates
Use OpenSSL or PowerShell to create:


Example using OpenSSL:
### Generate a CA certificate
```bash
openssl genrsa -out ca.key 2048
openssl req -x509 -new -nodes -key ca.key -sha256 -days 1024 -out ca.crt
```
### Generate a server certificate signed by the CA
```bash
openssl genrsa -out server.key 2048
openssl req -new -key server.key -out server.csr
openssl x509 -req -in server.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out server.crt -days 500 -sha256
```

### Generate a client certificate signed by the CA
```bash
openssl genrsa -out client.key 2048
openssl req -new -key client.key -out client.csr
openssl x509 -req -in client.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out client.crt -days 500 -sha256
```

## 2. Configure the ASP.NET Core Server
In Program.cs, configure Kestrel to require client certificates:

With the following code snippet, you can set up your ASP.NET Core application to use mTLS (mutual TLS) by configuring Kestrel to require client certificates and validate them against a specific CA or thumbprint.

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ServerCertificate = new X509Certificate2("server.pfx", "password");
        httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
        httpsOptions.ClientCertificateValidation = (cert, chain, errors) =>
        {
            // Optional: validate against CA or thumbprint
            return cert.Issuer == "CN=YourCA";
        };
    });
});
```csharp

With the following code snippet, you can set up your ASP.NET Core application to use mTLS (mutual TLS) by configuring Kestrel to require client certificates and validate them against a specific CA or thumbprint.

```csharp
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
```

## 3. Configure the Client

Use HttpClientHandler to attach the client certificate:

```csharp
var handler = new HttpClientHandler();
handler.ClientCertificates.Add(new X509Certificate2("client.pfx", "password"));
var client = new HttpClient(handler);
```

## 🧪 Test It Locally
You can run both server and client locally using self-signed certs.
