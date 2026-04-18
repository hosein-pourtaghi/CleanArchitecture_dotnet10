// src/LoggingLibrary/Services/TraceIdAccessor.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace SharedKernel.LoggingCore.Services;

public class TraceIdAccessor : ITraceIdAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TraceIdAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string TraceId => GetTraceId();
    public string CorrelationId => GetCorrelationId();

    private string GetTraceId()
    {
        var context = _httpContextAccessor.HttpContext;

        // 1. Check HttpContext.Items (set by middleware)
        if (context?.Items.TryGetValue("TraceId", out var traceId) == true
            && traceId is string trace)
        {
            return trace;
        }

        // 2. Fallback to Activity.Current
        if (!string.IsNullOrEmpty(Activity.Current?.Id))
        {
            return Activity.Current.Id;
        }

        // 3. Last resort
        return context?.TraceIdentifier ?? Guid.NewGuid().ToString();
    }

    private string GetCorrelationId()
    {
        var context = _httpContextAccessor.HttpContext;

        // 1. Check HttpContext.Items (set by middleware)
        if (context?.Items.TryGetValue("CorrelationId", out var correlationId) == true
            && correlationId is string correlation)
        {
            return correlation;
        }

        // 2. Fallback to header
        if (context?.Request.Headers.TryGetValue("X-Correlation-Id", out var header) == true)
        {
            return header.FirstOrDefault() ?? GetTraceId();
        }

        return GetTraceId();
    }
}
