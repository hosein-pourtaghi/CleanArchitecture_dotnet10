using MediatR;
using SharedKernel;

namespace Application.Common.DynamicCrud;


public sealed record DynamicCreateCommand<TEntity>(
    TEntity Entity
)
    : IRequest<Result<TEntity>>
    where TEntity : class;
