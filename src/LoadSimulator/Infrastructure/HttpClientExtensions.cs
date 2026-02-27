using System.Text.Json;
using System.Net.Http.Headers;

namespace LoadSimulator.Infrastructure;

/// <summary>
/// Extension methods for HTTP client operations
/// </summary>
public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Add bearer token to request
    /// </summary>
    public static HttpRequestMessage AddBearerToken(
        this HttpRequestMessage request, 
        string token)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    /// <summary>
    /// Deserialize JSON response
    /// </summary>
    public static async Task<T?> DeserializeAsync<T>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        
        if (string.IsNullOrWhiteSpace(content))
            return default;

        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Serialize object to JSON
    /// </summary>
    public static StringContent AsJsonContent<T>(this T obj)
    {
        var json = JsonSerializer.Serialize(obj, JsonOptions);
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Check if response is successful
    /// </summary>
    public static bool IsSuccessStatusCode(this HttpResponseMessage response) =>
        response.StatusCode >= System.Net.HttpStatusCode.OK &&
        response.StatusCode < System.Net.HttpStatusCode.BadRequest;
}
