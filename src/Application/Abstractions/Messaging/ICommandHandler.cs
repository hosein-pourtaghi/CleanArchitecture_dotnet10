using SharedKernel;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Handles a command that modifies state with no return value.
/// The command must implement ICommand (which returns Unit).
/// </summary>
/// <typeparam name="TCommand">The command type implementing ICommand</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the command execution.
    /// </summary>
    /// <param name="command">The command to handle</param>
    /// <param name="cancellationToken">Cancellation token for long-running operations</param>
    /// <returns>A result indicating success or failure</returns>
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Handles a command that modifies state and returns a response.
/// The command must implement ICommand&lt;TResponse&gt;.
/// </summary>
/// <typeparam name="TCommand">The command type implementing ICommand&lt;TResponse&gt;</typeparam>
/// <typeparam name="TResponse">The response type returned by the command</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Handles the command execution and returns a response.
    /// </summary>
    /// <param name="command">The command to handle</param>
    /// <param name="cancellationToken">Cancellation token for long-running operations</param>
    /// <returns>A result containing the response or an error</returns>
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
