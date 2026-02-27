using Polly;

namespace LoadSimulator.Infrastructure;

/// <summary>
/// Polly resilience policies for HTTP clients
/// </summary>
public static class PollyPolicies
{
    /// <summary>
    /// Exponential backoff retry policy: 3 retries with exponential backoff
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<OperationCanceledException>()
            .OrResult<HttpResponseMessage>(r =>
                r.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                r.StatusCode == System.Net.HttpStatusCode.BadGateway ||
                r.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                r.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(
                    Math.Pow(2, attempt) * 100),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Retry {retryCount} after {timespan.TotalMilliseconds}ms");
                });
    }

    /// <summary>
    /// Circuit breaker policy: Opens after 5 consecutive failures for 30 seconds
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r =>
                r.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Circuit breaker opened for {timespan.TotalSeconds}s");
                },
                onReset: () =>
                {
                    System.Diagnostics.Debug.WriteLine("Circuit breaker reset");
                });
    }

    /// <summary>
    /// Timeout policy: 30 seconds per request
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Combine all policies: Timeout + Retry + CircuitBreaker
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        return Policy.WrapAsync(
            GetTimeoutPolicy(),
            GetRetryPolicy(),
            GetCircuitBreakerPolicy());
    }
}
