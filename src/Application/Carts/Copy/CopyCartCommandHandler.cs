using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Carts;
using SharedKernel;

namespace Application.Carts.Copy;

/// <summary>
/// Handles cart creation.
/// Validates email uniqueness, creates cart entity, publishes domain event, and persists to database.
/// The domain event is published before persistence to ensure it's captured for audit logging and message bus publishing.
/// </summary>
internal sealed class CopyCartCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CopyCartCommand, bool>
{
    public async Task<Result<bool>> Handle(CopyCartCommand command, CancellationToken cancellationToken)
    {
        var index = 0;
        for (int j = 0; j < 20; j++)
        {
            var chunk = 10_000;
            for (int i = index; i < index + chunk; i++)
            {
                //var split = command.Email.Split("@");
                // split[0] = split[0] + i;

                //var email = string.Join("", split);

                // Validate email uniqueness
                //bool emailExists = await context.Carts
                //    .AnyAsync(c => c.Email == email, cancellationToken);

                //if (emailExists)
                //{
                //    continue;
                //}


                // Copy cart entity
                //var cart = new Cart
                //{
                //    Id = Guid.NewGuid(),
                //    Name = command.Name + " " + i,
                //    Email = email,
                //    Phone = command.Phone + i,
                //    Address = command.Address + " " + i,
                //    CreatedAt = DateTime.UtcNow
                //};

                //// Publish comprehensive domain event for audit logging and async operations (message bus)
                //cart.Raise(new CartCreatedDomainEvent(
                //    cartId: cart.Id,
                //    name: cart.Name,
                //    email: cart.Email,
                //    phone: cart.Phone,
                //    address: cart.Address));

                //// Persist to database
                //context.Carts.Add(cart);
            }
            await context.SaveChangesAsync(cancellationToken);
            index += chunk;
        }
        return true;
    }
}
