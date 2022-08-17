using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Gateway.Buckaroo;

public class HmacDelegatingHandler : DelegatingHandler
{
    private readonly BuckarooConfiguration _configuration;

    public HmacDelegatingHandler(IOptionsSnapshot<BuckarooConfiguration> configuration)
    {
        _configuration = configuration.Value;
    }

    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var requestContentBase64String = string.Empty;
        var requestUri = Uri.EscapeDataString(request.RequestUri!.Authority + request.RequestUri.PathAndQuery).ToLower();

        var requestTimeStamp = Convert.ToUInt64(DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString();

        var nonce = Guid.NewGuid().ToString("N");

        // checking if the request contains body, usually will be null with HTTP GET and DELETE
        if (request.Content != null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            var md5 = MD5.Create();

            // hashing the request body, any change in request body will result in different hash, we'll incure message integrity
            var requestContentHash = md5.ComputeHash(content);
            requestContentBase64String = Convert.ToBase64String(requestContentHash);
        }

        var signatureRawData = string.Format
            ("{0}{1}{2}{3}{4}{5}",
            _configuration.WebsiteKey,
            request.Method.Method,
            requestUri,
            requestTimeStamp,
            nonce,
            requestContentBase64String);

        var secretKeyByteArray = Encoding.UTF8.GetBytes(_configuration.PrivateKey);

        var signature = Encoding.UTF8.GetBytes(signatureRawData);

        using (var hmac = new HMACSHA256(secretKeyByteArray))
        {
            var signatureBytes = hmac.ComputeHash(signature);
            var requestSignatureBase64String = Convert.ToBase64String(signatureBytes);

            request.Headers.Authorization = new AuthenticationHeaderValue("hmac", string.Format(
                "{0}:{1}:{2}:{3}",
                _configuration.WebsiteKey,
                requestSignatureBase64String,
                nonce,
                requestTimeStamp));
        }

        var response = await base.SendAsync(request, cancellationToken);

        return response;
    }
}
