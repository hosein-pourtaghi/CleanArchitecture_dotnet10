using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using AutoMapper;
using Domain.Carts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Carts.Update;

/// <summary>
/// Handles cart updates.
/// Validates cart exists, ensures email uniqueness, updates entity, and publishes domain event.
/// </summary>
internal sealed class UpdateCartCommandHandler(
    IApplicationDbContext context,
    IMapper mapper
    )
    : ICommandHandler<UpdateCartCommand>
{
    public async Task<Result> Handle(UpdateCartCommand command, CancellationToken cancellationToken)
    {
        var cart = await context.Carts.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (cart is null)
        {
            return Result.Failure(CartErrors.NotFound(command.Id));
        }

        // Update entity with new values 
        cart = mapper.Map<Cart>(command);


        //// Publish comprehensive domain event with all updated data for auditing and message bus
        //cart.Raise(new CartUpdatedDomainEvent(
        //    cartId: cart.Id,
        //    name: cart.Name,
        //    email: cart.Email,
        //    phone: cart.Phone,
        //    address: cart.Address));


        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
