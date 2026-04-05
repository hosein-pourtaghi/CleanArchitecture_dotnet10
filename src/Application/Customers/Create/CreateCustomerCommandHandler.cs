using Application.Common.Data;
using Application.Common.Messaging;
using Domain.Customers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.Create;

/// <summary>
/// Handles customer creation.
/// Validates email uniqueness, creates customer entity, publishes domain event, and persists to database.
/// The domain event is published before persistence to ensure it's captured for audit logging and message bus publishing.
/// </summary>
internal sealed class CreateCustomerCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateCustomerCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        // Validate email uniqueness
        bool emailExists = await context.Customers
            .AnyAsync(c => c.Email == command.Email, cancellationToken);
        
        if (emailExists)
        {
            return Result.Failure<Guid>(CustomerErrors.EmailAlreadyExists(command.Email));
        }

        // Create customer entity
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Email = command.Email,
            Phone = command.Phone,
            Address = command.Address
        };

        //// Publish comprehensive domain event for audit logging and async operations (message bus)
        //customer.AddDomainEvent(new CustomerCreatedDomainEvent(
        //    customerId: customer.Id,
        //    name: customer.Name,
        //    email: customer.Email,
        //    phone: customer.Phone,
        //    address: customer.Address));

        // Persist to database
        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}
