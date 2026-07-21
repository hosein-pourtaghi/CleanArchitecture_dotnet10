using MediatR;
using SharedKernel;

namespace Application.Common.DynamicCrud;


public sealed record DynamicUpdateCommand<TEntity>(
    Guid Id,
    TEntity Entity
)
    : IRequest<Result>
    where TEntity : class;
