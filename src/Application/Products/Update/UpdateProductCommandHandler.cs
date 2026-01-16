
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using AutoMapper;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Update;
 
internal sealed class UpdateProductCommandHandler(
    IApplicationDbContext context,
    IMapper mapper
    )
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await context.Products.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(command.Id));
        }

        // Update entity with new values 
        product = mapper.Map<Product>(command);


        //// Publish comprehensive domain event with all updated data for auditing and message bus
        //product.Raise(new ProductUpdatedDomainEvent(
        //    productId: product.Id,
        //    name: product.Name,
        //    email: product.Email,
        //    phone: product.Phone,
        //    address: product.Address));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

