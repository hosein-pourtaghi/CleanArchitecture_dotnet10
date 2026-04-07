using Application.Common.Data;
using Application.Common.Messaging;
using Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.Update;

/// <summary>
/// Handles customer updates.
/// Validates customer exists, ensures email uniqueness, updates entity, and publishes domain event.
/// </summary>
internal sealed class UpdateCustomerCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateCustomerCommand>
{
    public async Task<Result> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await context.Customers.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (customer is null)
        {
            return Result.Failure(CustomerErrors.NotFound(command.Id));
        }

        // Validate email uniqueness if email is being changed
        if (!string.Equals(customer.Email, command.Email, StringComparison.OrdinalIgnoreCase))
        {
            bool emailExists = await context.Customers.AnyAsync(
                c => c.Email == command.Email && c.Id != command.Id,
                cancellationToken);
            if (emailExists)
            {
                return Result.Failure(CustomerErrors.EmailAlreadyExists(command.Email));
            }
        }

        // Update entity with new values
        customer.Name = command.Name;
        customer.Email = command.Email;
        customer.Phone = command.Phone;
        customer.Address = command.Address;

        //// Publish comprehensive domain event with all updated data for auditing and message bus
        //customer.Raise(new CustomerUpdatedDomainEvent(
        //    customerId: customer.Id,
        //    name: customer.Name,
        //    email: customer.Email,
        //    phone: customer.Phone,
        //    address: customer.Address));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
