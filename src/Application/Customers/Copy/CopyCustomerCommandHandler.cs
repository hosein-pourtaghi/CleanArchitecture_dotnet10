using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Customers;
using SharedKernel;

namespace Application.Customers.Copy;

/// <summary>
/// Handles customer creation.
/// Validates email uniqueness, creates customer entity, publishes domain event, and persists to database.
/// The domain event is published before persistence to ensure it's captured for audit logging and message bus publishing.
/// </summary>
internal sealed class CopyCustomerCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CopyCustomerCommand, bool>
{
    public async Task<Result<bool>> Handle(CopyCustomerCommand command, CancellationToken cancellationToken)
    {
        var index = 0;
        for (int j = 0; j < 20; j++)
        {
            var chunk = 10_000;
            for (int i = index; i < index + chunk; i++)
            {
                var split = command.Email.Split("@");
                 split[0] = split[0] + i;

                var email = string.Join("", split);

                // Validate email uniqueness
                //bool emailExists = await context.Customers
                //    .AnyAsync(c => c.Email == email, cancellationToken);

                //if (emailExists)
                //{
                //    continue;
                //}


                // Copy customer entity
                var customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    Name = command.Name + " " + i,
                    Email = email,
                    Phone = command.Phone + i,
                    Address = command.Address + " " + i,
                    CreatedAt = DateTime.UtcNow
                };

                // Publish comprehensive domain event for audit logging and async operations (message bus)
                customer.Raise(new CustomerCreatedDomainEvent(
                    customerId: customer.Id,
                    name: customer.Name,
                    email: customer.Email,
                    phone: customer.Phone,
                    address: customer.Address));

                // Persist to database
                context.Customers.Add(customer);
            }
            await context.SaveChangesAsync(cancellationToken);
            index += chunk;
        }
        return true;
    }
}
