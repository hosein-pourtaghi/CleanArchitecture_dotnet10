namespace Application.Abstractions.Messaging;

/// <summary>
/// Represents a query that reads data without modifying state.
/// Always returns a response of type TResponse.
/// Inherits from IRequest to enable pipeline behaviors.
/// </summary>
public interface IQuery<TResponse> : IRequest<TResponse>;
