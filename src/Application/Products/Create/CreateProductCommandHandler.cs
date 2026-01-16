
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using AutoMapper;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Create;

internal sealed class CreateProductCommandHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    { 
        // Create product entity
        var product = mapper.Map<Product>(command); 


        // Publish comprehensive domain event for audit logging and async operations (message bus)
        //product.Raise(new ProductCreatedDomainEvent(
        //    productId: product.Id,
        //    name: product.Name,
        //    email: product.Email,
        //    phone: product.Phone,
        //    address: product.Address));

        // Persist to database
        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}

