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

Example using Powershell:
### Generate a CA certificate
```ps
$params = @{
    Type = 'Custom'
    Subject = 'CN=MyLocalhostRootCert'
    KeySpec = 'Signature'
    KeyExportPolicy = 'Exportable'
    KeyUsage = 'CertSign'
    KeyUsageProperty = 'Sign'
    KeyLength = 2048
    HashAlgorithm = 'sha256'
    NotAfter = (Get-Date).AddMonths(24)
    CertStoreLocation = 'Cert:\CurrentUser\My'
}
$cert = New-SelfSignedCertificate @params
```
or find root certificate by name
```ps
 $certs = Get-ChildItem -path Cert:\* -Recurse | where {$_.Subject –like '*MyLocalhostRootCert*'}

 $certs.Length
 $cert = $certs[0]
 ```
 or by thumbprint
 ```ps
  $certs = Get-ChildItem -Path "Cert:\*<THUMBPRINT>" -Recurse

 $certs.Length
 $cert = $certs[0]
 ```
 #### Export CA certificate
 ```ps
 $cert | Export-Certificate -FilePath "ca.crt" -Type CERT
 $cert | Export-PfxCertificate -FilePath "ca.pfx" -Password (ConvertTo-SecureString -String "password" -AsPlainText -Force)
 ```
 OR manually export the certificate from the Windows Certificate Store:
 
* goto `run` and type `certmgr.msc`
* goto `Manage user certificates -> Certificates - Current Users` 
* goto `Personal -> Certificates`
* Right click on the root cetificate and follow the Wizard and  export with private key.
 
 #### Imoprt root certificate to tursted root
  ```ps
  $cert | Import-Certificate -CertStoreLocation "Cert:\LocalMachine\Root"
  ```
  Or manually import the certificate to the Trusted Root Certification Authorities store:
  
* goto `Manage user certificates -> Certificates - Current Users`
* goto `Trusted Root Certification Authorities -> Certificates`
* right click on import and follow the Wizard
* seelct `Trusted Root Certification Authorities` where necessary.

### Generate a server certificate signed by the CA
```ps
$params = @{
       Type = 'Custom'
       Subject = 'CN=localhost'
       DnsName = 'localhost'
       KeySpec = 'Signature'
       KeyExportPolicy = 'Exportable'
       KeyLength = 2048
       HashAlgorithm = 'sha256'
       NotAfter = (Get-Date).AddMonths(18)
       CertStoreLocation = 'Cert:\CurrentUser\My'
       Signer = $cert
       TextExtension = @(
        '2.5.29.37={text}1.3.6.1.5.5.7.3.1')
   }

New-SelfSignedCertificate @params
```

```ps
$params = @{
       Type = 'Custom'
       Subject = 'CN=myhost'
       DnsName = 'myhost'
       KeySpec = 'Signature'
       KeyExportPolicy = 'Exportable'
       KeyLength = 2048
       HashAlgorithm = 'sha256'
       NotAfter = (Get-Date).AddMonths(18)
       CertStoreLocation = 'Cert:\CurrentUser\My'
       Signer = $cert
       TextExtension = @(
        '2.5.29.37={text}1.3.6.1.5.5.7.3.1')
   }

 New-SelfSignedCertificate @params
```

#### Export client/dns certificate

* * goto `run` and type `certmgr.msc`
* goto `Manage user certificates -> Certificates - Current Users` 
* goto `Personal -> Certificates`
* Right click on the client/dns cetificate and follow the Wizard and  export with private key.
* Save as `localhost.pfx` in the `src\Local.ReverseProxy` folder and update the password in the ``appSettings.json`` file.
 

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
