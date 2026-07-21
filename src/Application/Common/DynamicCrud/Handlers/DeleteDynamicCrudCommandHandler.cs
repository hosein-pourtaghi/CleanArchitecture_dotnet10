using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Application.Common.DynamicCrud.Commands;


namespace Application.Common.DynamicCrud.Handlers;


public sealed class DeleteDynamicCrudCommandHandler<TEntity>
:
IRequestHandler<
    DeleteDynamicCrudCommand<TEntity>,
    Result>
where TEntity : class
{

    private readonly DbContext _db;


    public DeleteDynamicCrudCommandHandler(
        DbContext db)
    {
        _db = db;
    }



    public async Task<Result> Handle(
        DeleteDynamicCrudCommand<TEntity> request,
        CancellationToken cancellationToken)
    {

        var entity =
            await _db.Set<TEntity>()
                .FindAsync(
                    [request.Id],
                    cancellationToken);


        if (entity is null)
            return Result.Failure(
                Error.NotFound(
                    "Entity.NotFound",
                    "Entity not found"));


        _db.Remove(entity);


        await _db.SaveChangesAsync(
            cancellationToken);


        return Result.Success();
    }
}
