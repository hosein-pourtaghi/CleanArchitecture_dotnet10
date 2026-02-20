using System.Net.Http.Json;

namespace WebApi.Http;

public sealed class ExternalHttpClient : IExternalHttpClient
{
    private readonly HttpClient _httpClient;

    public ExternalHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<HttpResponseMessage> GetAsync(System.Uri relativeUrl, CancellationToken cancellationToken = default)
    {
        return _httpClient.GetAsync(relativeUrl, cancellationToken);
    }

    public Task<HttpResponseMessage> PostAsync<T>(System.Uri relativeUrl, T content, CancellationToken cancellationToken = default)
    {
        return _httpClient.PostAsJsonAsync(relativeUrl, content, cancellationToken);
    }
}
