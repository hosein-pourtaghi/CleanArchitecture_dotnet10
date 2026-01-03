using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Customers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.Delete;

/// <summary>
/// Handles customer deletion.
/// Validates customer exists, captures customer data before deletion, publishes domain event, and removes from database.
/// </summary>
internal sealed class DeleteCustomerCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteCustomerCommand>
{
    public async Task<Result> Handle(DeleteCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await context.Customers.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (customer is null)
        {
            return Result.Failure(CustomerErrors.NotFound(command.Id));
        }

        // Capture customer data before deletion for event publishing
        var deletedEvent = new CustomerDeletedDomainEvent(
            customerId: customer.Id,
            name: customer.Name,
            email: customer.Email,
            phone: customer.Phone,
            address: customer.Address);

        customer.Raise(deletedEvent);

        // Remove from database
        context.Customers.Remove(customer);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}