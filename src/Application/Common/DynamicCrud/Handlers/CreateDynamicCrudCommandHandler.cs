using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Application.Common.DynamicCrud.Commands;
using Application.Common.Interfaces.Core;

namespace Application.Common.DynamicCrud.Handlers;


public sealed class CreateDynamicCrudCommandHandler<TEntity>
    :
    IRequestHandler<
        CreateDynamicCrudCommand<TEntity>,
        Result<TEntity>>
    where TEntity : class
{

    private readonly IApplicationDbContext _db;


    public CreateDynamicCrudCommandHandler(
        IApplicationDbContext db)
    {
        _db = db;
    }



    public async Task<Result<TEntity>> Handle(
        CreateDynamicCrudCommand<TEntity> request,
        CancellationToken cancellationToken)
    {

        await _db.Set<TEntity>()
            .AddAsync(
                request.Entity,
                cancellationToken);


        await _db.SaveChangesAsync(
            cancellationToken);


        return Result.Success(
            request.Entity);
    }
}
