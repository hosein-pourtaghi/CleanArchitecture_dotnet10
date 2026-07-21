using Application.Common.DynamicCrud.Queries;
using Application.Common.Interfaces.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;


namespace Application.Common.DynamicCrud.Handlers;


public sealed class GetDynamicCrudByIdQueryHandler<TEntity>
:
IRequestHandler<
    GetDynamicCrudByIdQuery<TEntity>,
    Result<TEntity>>
where TEntity : class
{

    private readonly IApplicationDbContext _db;


    public GetDynamicCrudByIdQueryHandler(
        IApplicationDbContext db)
    {
        _db = db;
    }



    public async Task<Result<TEntity>> Handle(
        GetDynamicCrudByIdQuery<TEntity> request,
        CancellationToken cancellationToken)
    {

        var entity =
            await _db.Set<TEntity>()
                .FindAsync(
                    [request.Id],
                    cancellationToken);


        if (entity is null)
            return Result.Failure<TEntity>(
                Error.NotFound(
                    "Entity.NotFound",
                    "Entity not found"));


        return Result.Success(entity);
    }
}
