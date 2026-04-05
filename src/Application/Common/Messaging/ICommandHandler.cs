using MediatR;
using SharedKernel;

namespace Application.Common.Messaging;

/// <summary>
/// Handles a command that modifies state with no return value.
/// The command must implement ICommand (which returns Result).
/// Implements MediatR IRequestHandler for pipeline behavior support.
/// </summary>
/// <typeparam name="TCommand">The command type implementing ICommand</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Handles a command that modifies state and returns a response.
/// The command must implement ICommand&lt;TResponse&gt;.
/// Implements MediatR IRequestHandler for pipeline behavior support.
/// </summary>
/// <typeparam name="TCommand">The command type implementing ICommand&lt;TResponse&gt;</typeparam>
/// <typeparam name="TResponse">The response type returned by the command</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
