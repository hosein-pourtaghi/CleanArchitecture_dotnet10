using Application.Common.DynamicCrud.Queries;
using Application.Common.Interfaces.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;


namespace Application.Common.DynamicCrud.Handlers;


public sealed class GetDynamicCrudListQueryHandler<TEntity>
:
IRequestHandler<
    GetDynamicCrudListQuery<TEntity>,
    Result<IReadOnlyList<TEntity>>>
where TEntity : class
{

    private readonly IApplicationDbContext _db;


    public GetDynamicCrudListQueryHandler(
        IApplicationDbContext db)
    {
        _db = db;
    }



    public async Task<Result<IReadOnlyList<TEntity>>> Handle(
        GetDynamicCrudListQuery<TEntity> request,
        CancellationToken cancellationToken)
    {

        var items =
            await _db.Set<TEntity>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);


        return Result.Success<IReadOnlyList<TEntity>>(
            items);
    }
}
