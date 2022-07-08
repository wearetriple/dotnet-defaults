using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gateway.Buckaroo;

internal class PspClient : IPspClient
{
    private readonly HttpClient _client;
    private readonly ILogger<PspClient> _logger;
    private readonly BuckarooConfiguration _configuration;

    public PspClient(HttpClient client, ILogger<PspClient> logger, IOptionsSnapshot<BuckarooConfiguration> configuration)
    {
        _client = client;
        _logger = logger;
        _configuration = configuration.Value;
    }

    public async Task<TResponse> PostTransactionAsync<TRequest, TResponse>(TRequest transactionRequest)
        => await PostRequestAsync<TResponse, TRequest>(transactionRequest, "json/transaction");

    public async Task<TResponse> PostDataRequestAsync<TRequest, TResponse>(TRequest dataRequest)
        => await PostRequestAsync<TResponse, TRequest>(dataRequest, "json/datarequest");

    private async Task<TResponse> PostRequestAsync<TResponse, TRequest>(TRequest request, string slug)
    {
        try
        {
            var httpResponse = await _client.PostAsJsonAsync(_configuration.BaseUrl + slug, request);

            httpResponse.EnsureSuccessStatusCode();

            var contentStream = await httpResponse.Content.ReadAsStreamAsync();

            var buckarooResponse = await JsonSerializer.DeserializeAsync<TResponse>(contentStream);

            return buckarooResponse ?? throw new Exception("Failed to serialize into object");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to do a {request} : {exceptionMessage}", nameof(TRequest), ex.Message);

            throw;
        }
    }
}
