using MediatR;
using SharedKernel;

namespace Application.Common.DynamicCrud;


public sealed record DynamicGetByIdQuery<TEntity>(
    Guid Id
)
    : IRequest<Result<TEntity>>
    where TEntity : class;
