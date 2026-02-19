using MediatR;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Base interface for all requests (commands and queries).
/// Enables MediatR pipeline behaviors and cross-cutting concerns for both command and query handling.
/// Extends MediatR.IRequest for framework integration.
/// </summary>
public interface IRequest<TResponse> : MediatR.IRequest<TResponse>;
