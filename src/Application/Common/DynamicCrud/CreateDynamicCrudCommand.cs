using MediatR;
using SharedKernel;

namespace Application.Common.DynamicCrud.Commands;

public sealed record CreateDynamicCrudCommand<TEntity>(
    TEntity Entity
)
    : IRequest<Result<TEntity>>
    where TEntity : class;
