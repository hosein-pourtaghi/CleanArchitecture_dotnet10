using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Application.Common.DynamicCrud.Commands;


namespace Application.Common.DynamicCrud.Handlers;


public sealed class UpdateDynamicCrudCommandHandler<TEntity>
:
IRequestHandler<
    UpdateDynamicCrudCommand<TEntity>,
    Result>
where TEntity : class
{

    private readonly DbContext _db;


    public UpdateDynamicCrudCommandHandler(
        DbContext db)
    {
        _db = db;
    }



    public async Task<Result> Handle(
        UpdateDynamicCrudCommand<TEntity> request,
        CancellationToken cancellationToken)
    {

        var exists =
            await _db.Set<TEntity>()
                .FindAsync(
                    [request.Id],
                    cancellationToken);


        if (exists is null)
            return Result.Failure(
                Error.NotFound(
                    "Entity.NotFound",
                    "Entity not found"));


        _db.Entry(exists)
            .CurrentValues
            .SetValues(request.Entity);


        await _db.SaveChangesAsync(
            cancellationToken);


        return Result.Success();
    }
}
