using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Application.Common.DynamicCrud.Queries;


namespace Application.Common.DynamicCrud.Handlers;


public sealed class GetDynamicCrudByIdQueryHandler<TEntity>
:
IRequestHandler<
    GetDynamicCrudByIdQuery<TEntity>,
    Result<TEntity>>
where TEntity : class
{

    private readonly DbContext _db;


    public GetDynamicCrudByIdQueryHandler(
        DbContext db)
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
