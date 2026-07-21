using MediatR;
using SharedKernel;

namespace Application.Common.DynamicCrud.Commands;


public sealed record DeleteDynamicCrudCommand<TEntity>(
    Guid Id
)
    : IRequest<Result>
    where TEntity : class;
