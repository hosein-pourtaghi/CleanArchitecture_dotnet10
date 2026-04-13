using MediatR;
using SharedKernel;

namespace Application.Common.Messaging;

public interface IQuery { }
/// <summary>
/// Represents a query that reads data without modifying state.
/// Always returns a response of type TResponse wrapped in a Result.
/// Inherits from IRequest to enable MediatR pipeline behaviors.
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
