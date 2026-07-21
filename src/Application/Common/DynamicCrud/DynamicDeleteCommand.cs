using MediatR;
using SharedKernel;

namespace Application.Common.DynamicCrud;


public sealed record DynamicDeleteCommand<TEntity>(
    Guid Id
)
    : IRequest<Result>
    where TEntity : class;
