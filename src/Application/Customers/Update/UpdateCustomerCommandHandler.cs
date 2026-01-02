using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Customers.Update;

internal sealed class UpdateCustomerCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateCustomerCommand>
{
    public async Task<Result> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await context.Customers.SingleOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (customer is null)
        {
            return Result.Failure(Error.NotFound("Customers.NotFound", $"Customer with Id '{command.Id}' was not found"));
        }

        // email uniqueness check if changed
        if (!string.Equals(customer.Email, command.Email, StringComparison.OrdinalIgnoreCase))
        {
            bool emailExists = await context.Customers.AnyAsync(c => c.Email == command.Email && c.Id != command.Id, cancellationToken);
            if (emailExists)
            {
                return Result.Failure(Error.Conflict("Customers.EmailExists", $"Customer with email '{command.Email}' already exists."));
            }
        }

        customer.Name = command.Name;
        customer.Email = command.Email;
        customer.Phone = command.Phone;
        customer.Address = command.Address;
        customer.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}