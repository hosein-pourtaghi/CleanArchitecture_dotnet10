using SharedKernel;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Represents a command that modifies state with no return value.
/// Inherits from IRequest to enable pipeline behaviors.
/// </summary>
public interface ICommand : IRequest<Unit>;

/// <summary>
/// Represents a command that modifies state and returns a response of type TResponse.
/// Inherits from IRequest to enable pipeline behaviors.
/// </summary>
public interface ICommand<TResponse> : IRequest<TResponse>;
