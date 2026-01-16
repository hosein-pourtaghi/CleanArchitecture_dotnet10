using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Delete;
 
internal sealed class DeleteProductCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await context.Products.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(command.Id));
        }

        // Capture product data before deletion for event publishing
        //var deletedEvent = new ProductDeletedDomainEvent(
        //    productId: product.Id,
        //    name: product.Name,
        //    email: product.Email,
        //    phone: product.Phone,
        //    address: product.Address);
        //product.Raise(deletedEvent);

        // Remove from database
        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
