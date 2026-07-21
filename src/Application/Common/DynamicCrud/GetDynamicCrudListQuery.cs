using MediatR;
using SharedKernel;

namespace Application.Common.DynamicCrud.Queries;


public sealed record GetDynamicCrudListQuery<TEntity>
    : IRequest<Result<IReadOnlyList<TEntity>>>
    where TEntity : class;
