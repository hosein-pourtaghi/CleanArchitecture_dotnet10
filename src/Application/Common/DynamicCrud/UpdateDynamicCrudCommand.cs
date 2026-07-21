using MediatR;
using SharedKernel;

namespace Application.Common.DynamicCrud.Commands;

public sealed record UpdateDynamicCrudCommand<TEntity>(
    Guid Id,
    TEntity Entity
)
    : IRequest<Result>
    where TEntity : class;
