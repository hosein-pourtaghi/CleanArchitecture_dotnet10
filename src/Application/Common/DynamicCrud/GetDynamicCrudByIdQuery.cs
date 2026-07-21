using MediatR;
using SharedKernel;

namespace Application.Common.DynamicCrud.Queries;


public sealed record GetDynamicCrudByIdQuery<TEntity>(
    Guid Id
)
    : IRequest<Result<TEntity>>
    where TEntity : class;
