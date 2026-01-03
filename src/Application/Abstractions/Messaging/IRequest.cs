namespace Application.Abstractions.Messaging;

/// <summary>
/// Base interface for all requests (commands and queries).
/// Enables pipeline behaviors and cross-cutting concerns for both command and query handling.
/// </summary>
public interface IRequest<TResponse>;
