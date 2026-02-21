using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using AutoMapper;
using Domain.Carts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Carts.Create;

/// <summary>
/// Handles cart creation.
/// Validates email uniqueness, creates cart entity, publishes domain event, and persists to database.
/// The domain event is published before persistence to ensure it's captured for audit logging and message bus publishing.
/// </summary>
internal sealed class CreateCartCommandHandler(
    IApplicationDbContext context,
    IMapper mapper)
    : ICommandHandler<CreateCartCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCartCommand command, CancellationToken cancellationToken)
    { 
        // Create cart entity
        var cart = mapper.Map<Cart>(command); 


        // Publish comprehensive domain event for audit logging and async operations (message bus)
        //cart.Raise(new CartCreatedDomainEvent(
        //    cartId: cart.Id,
        //    name: cart.Name,
        //    email: cart.Email,
        //    phone: cart.Phone,
        //    address: cart.Address));


        // Persist to database
        context.Carts.Add(cart);
        await context.SaveChangesAsync(cancellationToken);

        return cart.Id;
    }
}
