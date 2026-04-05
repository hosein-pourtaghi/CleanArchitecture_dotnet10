using MediatR;
using SharedKernel;

namespace Application.Common.Messaging;

/// <summary>
/// Represents a command that modifies state with no return value.
/// Inherits from IRequest to enable MediatR pipeline behaviors.
/// </summary>
public interface ICommand : IRequest<Result>;

/// <summary>
/// Represents a command that modifies state and returns a response of type TResponse.
/// Inherits from IRequest to enable MediatR pipeline behaviors.
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
