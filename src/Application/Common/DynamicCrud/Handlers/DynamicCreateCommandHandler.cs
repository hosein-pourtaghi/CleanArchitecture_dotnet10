using Application.Common.DynamicCrud;
using Application.Common.Interfaces.Core;
using MediatR;
using SharedKernel;


public sealed class DynamicCreateCommandHandler<TEntity>
    : IRequestHandler<
        DynamicCreateCommand<TEntity>,
        Result<TEntity>>
    where TEntity : class
{

    private readonly IApplicationDbContext _db;


    public DynamicCreateCommandHandler(
        IApplicationDbContext db)
    {
        _db = db;
    }



    public async Task<Result<TEntity>> Handle(
        DynamicCreateCommand<TEntity> request,
        CancellationToken cancellationToken)
    {

        _db.Set<TEntity>()
            .Add(request.Entity);


        await _db.SaveChangesAsync(
            cancellationToken);


        return Result.Success(
            request.Entity);
    }
}
