using System.Transactions;
using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

public class TransactionCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _decorated;

    public TransactionCommandHandlerDecorator(ICommandHandler<TCommand> decorated)
    {
        _decorated = decorated;
    }

    public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
    {
        using var transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted, // <--- Add this
                Timeout = TimeSpan.FromSeconds(30)
            },
            TransactionScopeAsyncFlowOption.Enabled);

        // Execute the actual handler logic
        Result result = await _decorated.Handle(command, cancellationToken);

        // Only commit if the handler succeeded
        if (result.IsSuccess)
        {
            // Mark transaction as complete
            transactionScope.Complete();
        }

        return result;

    }
}

// Decorator for handlers that return a value (TResponse)
public class TransactionCommandHandlerDecorator<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly ICommandHandler<TCommand, TResponse> _decorated;

    public TransactionCommandHandlerDecorator(ICommandHandler<TCommand, TResponse> decorated)
    {
        _decorated = decorated;
    }

    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {

        using var transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted, // <--- Add this
                Timeout = TimeSpan.FromSeconds(30)
            },
            TransactionScopeAsyncFlowOption.Enabled);

        // Execute the actual handler logic
        Result<TResponse> result = await _decorated.Handle(command, cancellationToken);

        // Only commit if the handler succeeded
        if (result.IsSuccess)
        {
            // Mark transaction as complete
            transactionScope.Complete();
        }

        return result;

    }
}
