// Infrastructure/Messaging/NotificationApiOptions.cs 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Messaging;

/// <summary>
/// Configuration options for the NotificationAPI integration
/// </summary>
public sealed class NotificationApiOptions
{
    public const string SectionName = "NotificationApi";

    /// <summary>
    /// Base URL of the NotificationAPI
    /// </summary>
    public required string BaseUrl { get; init; }

    /// <summary>
    /// API key for authentication with NotificationAPI
    /// </summary>
    public required string ApiKey { get; init; }

    /// <summary>
    /// Timeout for HTTP requests in seconds
    /// </summary>
    public int TimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Number of retry attempts for failed requests
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Endpoint configuration
    /// </summary>
    public EndpointsOptions Endpoints { get; init; } = new();
}

public sealed class EndpointsOptions
{
    /// <summary>
    /// Endpoint for sending notifications
    /// </summary>
    public string Notifications { get; init; } = "/api/notifications/events";

    /// <summary>
    /// Health check endpoint
    /// </summary>
    public string Health { get; init; } = "/health";

    /// <summary>
    /// Endpoint for batch notifications
    /// </summary>
    public string BatchNotifications { get; init; } = "/api/notifications/events/batch";
}
