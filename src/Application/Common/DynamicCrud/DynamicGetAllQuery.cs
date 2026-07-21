using MediatR;
using SharedKernel;

namespace Application.Common.DynamicCrud;


public sealed record DynamicGetAllQuery<TEntity>
    : IRequest<Result<IReadOnlyList<TEntity>>>
    where TEntity : class;
